using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.Models.ChangeTypes;
using SplunkBulkUpdater.Application.Models.SavedSearches;
using SplunkBulkUpdater.Application.UseCases.BulkUpdateSplString;
using SplunkBulkUpdater.Application.UseCases.BulkUpdateAlertActions;
using SplunkBulkUpdater.Application.UseCases.Common;
using SplunkBulkUpdater.Application.UseCases.GetAlerts;
using SplunkBulkUpdater.Application.UseCases.RetrieveApps;
using SplunkBulkUpdater.Application.UseCases.RetrieveChangeFields;
using SplunkBulkUpdater.Application.UseCases.RetrieveChangeTypes;
using SplunkBulkUpdater.WPF.Dialogs;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace SplunkBulkUpdater.WPF.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly ICommandDispatcher _dispatcher;
    private readonly IReplacementConfirmationService _confirmationService;
    private readonly IAlertActionConfirmationService _actionConfirmationService;
    private readonly string _signedInUser;

    public MainWindowViewModel(
        ICommandDispatcher dispatcher,
        IReplacementConfirmationService confirmationService,
        IAlertActionConfirmationService actionConfirmationService,
        SignedInUserContext userContext)
    {
        _dispatcher = dispatcher;
        _confirmationService = confirmationService;
        _actionConfirmationService = actionConfirmationService;
        _signedInUser = userContext.Username;
        OwnerFilters =
        [
            new OwnerFilterOption("All owners", "-"),
            new OwnerFilterOption("Shared searches", "nobody"),
            new OwnerFilterOption("My searches", _signedInUser)
        ];
        SelectedOwnerFilter = OwnerFilters.Last();
    }

    [ObservableProperty] private string app = "search";
    [ObservableProperty] private string splRegex = string.Empty;
    [ObservableProperty] private string replacement = string.Empty;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string statusMessage = "Sign in succeeded. Enter search criteria.";
    [ObservableProperty] private Brush statusBrush = Brushes.DimGray;
    [ObservableProperty] private SelectableSavedSearch? selectedMatch;
    [ObservableProperty] private string? selectedApp;
    [ObservableProperty] private ChangeType? selectedChangeType;
    [ObservableProperty] private OwnerFilterOption? selectedOwnerFilter;
    [ObservableProperty] private bool isSplSelected;
    [ObservableProperty] private bool isActionTypeSelected;

    public ObservableCollection<string> Apps { get; } = [];
    public ObservableCollection<ChangeType> ChangeTypes { get; } = [];
    public ObservableCollection<OwnerFilterOption> OwnerFilters { get; }
    public ObservableCollection<SelectableSavedSearch> Matches { get; } = [];
    public ObservableCollection<ChangeFieldEditor> ChangeFields { get; } = [];
    public ObservableCollection<SplGroupEditor> SplGroups { get; } = [];
    public int SelectedMatchCount => Matches.Count(item => item.IsSelected);

    partial void OnSelectedAppChanged(string? value) => NotifyCommands();
    partial void OnSplRegexChanged(string value)
    {
        NotifyCommands();
        RefreshSplGroups(value);
    }
    partial void OnReplacementChanged(string value) => NotifyCommands();
    partial void OnSelectedOwnerFilterChanged(OwnerFilterOption? value) => NotifyCommands();
    partial void OnSelectedChangeTypeChanged(ChangeType? value)
    {
        IsSplSelected = value == ChangeType.SPL;
        IsActionTypeSelected = value is not null && value != ChangeType.SPL;
        _ = LoadChangeFieldsAsync(value);
        NotifyCommands();
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            var apps = await _dispatcher.Send(new RetrieveAppsCommand(), CancellationToken.None);
            if (apps.IsSuccess && apps.Payload is not null)
            {
                Apps.Clear();
                foreach (var appName in apps.Payload.Apps.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                    Apps.Add(appName);
                SelectedApp = Apps.FirstOrDefault() ?? "search";
            }
            else ShowResultErrors(apps);

            var types = await _dispatcher.Send(new RetrieveChangeTypesCommand(), CancellationToken.None);
            if (types.IsSuccess && types.Payload is not null)
            {
                ChangeTypes.Clear();
                foreach (var type in types.Payload.ChangeTypes)
                    ChangeTypes.Add(type);
                SelectedChangeType = ChangeTypes.FirstOrDefault();
            }
            else ShowResultErrors(types);
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; NotifyCommands(); }
    }

    [RelayCommand]
    private async Task FindMatchesAsync()
    {
        if (!ValidateSearchInputs()) return;
        IsBusy = true;
        StatusMessage = "Retrieving matching alerts from Splunk...";
        StatusBrush = Brushes.DimGray;
        Matches.Clear();
        try
        {
            var result = await _dispatcher.Send(
                new GetAlertsCommand(SplRegex, SelectedApp!, SelectedOwnerFilter!.Owner, SelectedChangeType),
                CancellationToken.None);
            if (!result.IsSuccess || result.Payload is null) { ShowResultErrors(result); return; }

            foreach (var match in result.Payload)
            {
                var row = new SelectableSavedSearch(match.Id, match.SplSearchString);
                row.PropertyChanged += (_, args) =>
                {
                    if (args.PropertyName == nameof(SelectableSavedSearch.IsSelected))
                        OnPropertyChanged(nameof(SelectedMatchCount));
                };
                Matches.Add(row);
            }

            OnPropertyChanged(nameof(SelectedMatchCount));
            StatusMessage = $"Found {Matches.Count} matching alert(s). Select the entries to include in the next step.";
            StatusBrush = Brushes.DarkGreen;
        }
        catch (RegexMatchTimeoutException) { SetError("The regex took too long to evaluate."); }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; NotifyCommands(); }
    }

    [RelayCommand]
    private async Task ApplyReplacementAsync()
    {
        var selectedIds = Matches.Where(item => item.IsSelected).Select(item => item.Id).ToHashSet();
        if (selectedIds.Count == 0) { SetError("Select at least one alert before continuing."); return; }
        if (!ValidateSearchInputs()) return;

        IsBusy = true;
        try
        {
            if (SelectedChangeType is not ChangeType.SPL)
            {
                await ApplyActionReplacementAsync(selectedIds);
                return;
            }
            var groupReplacements = SplGroups
                .Where(group => !string.IsNullOrEmpty(group.ReplacementValue))
                .ToDictionary(group => group.GroupName, group => group.ReplacementValue, StringComparer.Ordinal);
            if (groupReplacements.Count == 0) { SetError("Enter a replacement value for at least one named SPL group."); return; }

            var previewCommand = new BulkUpdateSplCommand(
                SplRegex, Replacement, SelectedApp, SelectedOwnerFilter!.Owner,
                Preview: true, GroupReplacements: groupReplacements);
            var previewResult = await _dispatcher.Send(previewCommand, CancellationToken.None);
            if (!previewResult.IsSuccess || previewResult.Payload is null) { ShowResultErrors(previewResult); return; }

            var selectedPreview = previewResult.Payload.Changes
                .Where(change => selectedIds.Contains(change.Id)).ToList();
            if (selectedPreview.Count == 0) { StatusMessage = "No selected alerts would change."; StatusBrush = Brushes.DarkOrange; return; }

            var confirmedChanges = _confirmationService.Confirm(selectedPreview);
            if (confirmedChanges is null) { StatusMessage = "Update cancelled. No changes were made."; StatusBrush = Brushes.DimGray; return; }
            if (confirmedChanges.Count == 0) { StatusMessage = "No entries were selected in the review."; StatusBrush = Brushes.DarkOrange; return; }

            var updateResult = await _dispatcher.Send(
                previewCommand with { Preview = false, ConfirmedChanges = confirmedChanges },
                CancellationToken.None);
            if (!updateResult.IsSuccess || updateResult.Payload is null) { ShowResultErrors(updateResult); return; }

            StatusMessage = $"Update complete. Succeeded: {updateResult.Payload.SucceededCount}; failed: {updateResult.Payload.FailedCount}.";
            StatusBrush = updateResult.Payload.FailedCount == 0 ? Brushes.DarkGreen : Brushes.DarkOrange;
            await FindMatchesAsync();
        }
        catch (RegexMatchTimeoutException) { SetError("The regex took too long to evaluate."); }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; NotifyCommands(); }
    }

    private async Task ApplyActionReplacementAsync(IReadOnlySet<SavedSearchId> selectedIds)
    {
        var changes = ChangeFields
            .Where(field => field.IsSelected && !string.IsNullOrWhiteSpace(field.MatchRegex))
            .ToDictionary(field => field.Key,
                field => new RegexReplacement(field.MatchRegex, field.ReplacementValue),
                StringComparer.Ordinal);
        if (changes.Count == 0)
        {
            SetError("Check at least one action setting and enter its match regex.");
            return;
        }

        var previewCommand = new BulkUpdateAlertActionsCommand(
            SelectedApp!, SelectedOwnerFilter!.Owner, SelectedChangeType!.Value,
            changes, Preview: true, SelectedSearches: selectedIds);
        var previewResult = await _dispatcher.Send(previewCommand, CancellationToken.None);
        if (!previewResult.IsSuccess || previewResult.Payload is null)
        {
            ShowResultErrors(previewResult);
            return;
        }
        if (previewResult.Payload.Changes.Count == 0)
        {
            StatusMessage = "No selected alerts contain a matching action setting.";
            StatusBrush = Brushes.DarkOrange;
            return;
        }

        var confirmed = _actionConfirmationService.Confirm(previewResult.Payload.Changes);
        if (confirmed is null)
        {
            StatusMessage = "Update cancelled. No changes were made.";
            StatusBrush = Brushes.DimGray;
            return;
        }
        if (confirmed.Count == 0)
        {
            StatusMessage = "No entries were selected in the review.";
            StatusBrush = Brushes.DarkOrange;
            return;
        }

        var updateResult = await _dispatcher.Send(
            previewCommand with { Preview = false, ConfirmedChanges = confirmed },
            CancellationToken.None);
        if (!updateResult.IsSuccess || updateResult.Payload is null)
        {
            ShowResultErrors(updateResult);
            return;
        }

        StatusMessage = $"Update complete. Succeeded: {updateResult.Payload.SucceededCount}; failed: {updateResult.Payload.FailedCount}.";
        StatusBrush = updateResult.Payload.FailedCount == 0 ? Brushes.DarkGreen : Brushes.DarkOrange;
        await FindMatchesAsync();
    }

    private async Task LoadChangeFieldsAsync(ChangeType? changeType)
    {
        ChangeFields.Clear();
        if (changeType is null) return;
        if (changeType == ChangeType.SPL)
        {
            ChangeFields.Add(new ChangeFieldEditor("SPL", "SPL expression"));
            return;
        }
        try
        {
            var result = await _dispatcher.Send(new RetrieveChangeFieldsCommand(changeType.Value), CancellationToken.None);
            if (!result.IsSuccess || result.Payload is null) { ShowResultErrors(result); return; }
            foreach (var field in result.Payload.Fields.OrderBy(item => item.Value, StringComparer.OrdinalIgnoreCase))
                ChangeFields.Add(new ChangeFieldEditor(field.Key, field.Value));
        }
        catch (Exception ex) { SetError(ex.Message); }
    }

    private void RefreshSplGroups(string pattern)
    {
        var previousValues = SplGroups.ToDictionary(
            group => group.GroupName, group => group.ReplacementValue, StringComparer.Ordinal);
        SplGroups.Clear();
        if (string.IsNullOrWhiteSpace(pattern)) return;

        try
        {
            var regex = new Regex(pattern, RegexOptions.None, TimeSpan.FromSeconds(1));
            foreach (var name in regex.GetGroupNames().Where(name => !int.TryParse(name, out _)))
                SplGroups.Add(new SplGroupEditor(name, previousValues.GetValueOrDefault(name, string.Empty)));
        }
        catch (ArgumentException)
        {
            // No group editor is shown until the user enters a valid regex.
        }
    }

    private bool ValidateSearchInputs()
    {
        if (string.IsNullOrWhiteSpace(SelectedApp)) { SetError("Select a Splunk app."); return false; }
        if (SelectedOwnerFilter is null) { SetError("Select an owner filter."); return false; }
        if (string.IsNullOrWhiteSpace(SplRegex)) { SetError("Enter the SPL regular expression to find."); return false; }
        return true;
    }

    private void ShowResultErrors(BaseResult result)
    {
        var errors = result.ValidationErrors?.Select(error => error.Message).ToArray();
        SetError(errors is { Length: > 0 } ? string.Join(Environment.NewLine, errors) : result.Message ?? "The operation failed.");
    }

    private void SetError(string message) { StatusMessage = message; StatusBrush = Brushes.Firebrick; }
    private void NotifyCommands() { FindMatchesCommand.NotifyCanExecuteChanged(); ApplyReplacementCommand.NotifyCanExecuteChanged(); }
}

public sealed record OwnerFilterOption(string Label, string Owner)
{
    public override string ToString() => Label;
}

public partial class SelectableSavedSearch : ObservableObject
{
    public SelectableSavedSearch(SavedSearchId id, string splSearchString)
    { Id = id; SplSearchString = splSearchString; }

    public SavedSearchId Id { get; }
    public string SplSearchString { get; }
    [ObservableProperty] private bool isSelected = true;
}