using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SplunkBulkUpdater.Application.Models.ChangeTypes;
using SplunkBulkUpdater.Application.Models.SavedSearches;
using SplunkBulkUpdater.Application.UseCases.BulkUpdateAlertActions;
using SplunkBulkUpdater.Application.UseCases.BulkUpdateSplString;
using SplunkBulkUpdater.Application.UseCases.RetrieveChangeFields;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace SplunkBulkUpdater.WPF.ViewModels;

public partial class MainWindowViewModel
{
    [ObservableProperty] private string replacement = string.Empty;
    [ObservableProperty] private ChangeType? selectedChangeType;
    [ObservableProperty] private bool isSplSelected;
    [ObservableProperty] private bool isActionTypeSelected;

    public ObservableCollection<ChangeType> ChangeTypes { get; } = [];
    public ObservableCollection<ChangeFieldEditor> ChangeFields { get; } = [];

    partial void OnReplacementChanged(string value) => NotifyCommands();

    partial void OnSelectedChangeTypeChanged(ChangeType? value)
    {
        IsSplSelected = value == ChangeType.SPL;
        IsActionTypeSelected = value is not null && value != ChangeType.SPL;
        _ = LoadChangeFieldsAsync(value);
        NotifyCommands();
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

            var searchReplacement = ChangeFields.SingleOrDefault();
            if (searchReplacement is null)
            {
                SetError("Only one change field expected, but found more."); return;
            }

            ReplacementPair replacementPair = new(searchReplacement.MatchSubstring, searchReplacement.ReplacementValue);
            var previewCommand = new BulkUpdateSplCommand(
                SelectedApp!,
                SelectedOwnerFilter!.Owner,
                replacementPair,
                selectedIds,
                Preview: true
                //add filters as dict after implemented in UI);
                );
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
            .Where(field => field.IsSelected && !string.IsNullOrWhiteSpace(field.MatchSubstring))
            .ToDictionary(field => field.Key,
                field => new ReplacementPair(field.MatchSubstring, field.ReplacementValue),
                StringComparer.Ordinal);
        if (changes.Count == 0)
        {
            SetError("Check at least one action setting and enter its match substring.");
            return;
        }

        var previewCommand = new BulkUpdateAlertActionsCommand(
            SelectedApp!,
            SelectedOwnerFilter!.Owner,
            SelectedChangeType!.Value,
            changes, Preview: true,
            SelectedSearches: selectedIds
            //SearchStringFilters: adaug dictul ala cand va fi gata.
            );
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
        try
        {
            var result = await _dispatcher.Send(new RetrieveChangeFieldsCommand(changeType.Value), CancellationToken.None);
            if (!result.IsSuccess || result.Payload is null) { ShowResultErrors(result); return; }
            foreach (var field in result.Payload.Fields.OrderBy(item => item.Value, StringComparer.OrdinalIgnoreCase))
                ChangeFields.Add(new ChangeFieldEditor(field.Key, field.Value));
        }
        catch (Exception ex) { SetError(ex.Message); }
    }
}
