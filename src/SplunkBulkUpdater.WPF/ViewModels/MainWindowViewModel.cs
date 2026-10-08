using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.Models.SavedSearches;
using SplunkBulkUpdater.Application.UseCases.BulkUpdateSplString;
using SplunkBulkUpdater.Application.UseCases.Common;
using SplunkBulkUpdater.Application.UseCases.GetAlerts;
using SplunkBulkUpdater.WPF.Dialogs;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace SplunkBulkUpdater.WPF.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly ICommandDispatcher _dispatcher;
    private readonly IReplacementConfirmationService _confirmationService;

    public MainWindowViewModel(
        ICommandDispatcher dispatcher,
        IReplacementConfirmationService confirmationService)
    {
        _dispatcher = dispatcher;
        _confirmationService = confirmationService;
    }

    [ObservableProperty]
    private string app = "search";

    [ObservableProperty]
    private string owner = "nobody";

    [ObservableProperty]
    private string splRegex = string.Empty;

    [ObservableProperty]
    private string replacement = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string statusMessage = "Sign in succeeded. Enter search criteria.";

    [ObservableProperty]
    private Brush statusBrush = Brushes.DimGray;

    [ObservableProperty]
    private SavedSearch? selectedMatch;

    public ObservableCollection<SavedSearch> Matches { get; } = [];

    partial void OnAppChanged(string value) => NotifyCommands();
    partial void OnOwnerChanged(string value) => NotifyCommands();
    partial void OnSplRegexChanged(string value) => NotifyCommands();
    partial void OnReplacementChanged(string value) => NotifyCommands();

    [RelayCommand]
    private async Task FindMatchesAsync()
    {
        if (!ValidateSearchInputs())
            return;

        IsBusy = true;
        StatusMessage = "Retrieving matching alerts from Splunk...";
        StatusBrush = Brushes.DimGray;
        Matches.Clear();

        try
        {
            var command = new GetAlertsCommand(
                SplRegex,
                App.Trim(),
                Owner.Trim());

            var result = await _dispatcher.Send(
                command,
                CancellationToken.None);

            if (!result.IsSuccess || result.Payload is null)
            {
                ShowResultErrors(result);
                return;
            }

            foreach (var match in result.Payload)
            {
                Matches.Add(new SavedSearch(
                    match.Id,
                    match.SplSearchString));
            }

            StatusMessage = $"Found {Matches.Count} matching alert(s).";
            StatusBrush = Brushes.DarkGreen;
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    [RelayCommand]
    private async Task ApplyReplacementAsync()
    {
        if (!ValidateSearchInputs())
            return;

        if (string.IsNullOrWhiteSpace(Replacement))
        {
            SetError("Enter the replacement expression.");
            return;
        }

        IsBusy = true;
        StatusMessage = "Preparing the before-and-after preview...";
        StatusBrush = Brushes.DimGray;

        try
        {
            var previewCommand = new BulkUpdateSplCommand(
                SplRegex,
                Replacement,
                App.Trim(),
                Owner.Trim(),
                Preview: true);

            var previewResult = await _dispatcher.Send(
                previewCommand,
                CancellationToken.None);

            if (!previewResult.IsSuccess || previewResult.Payload is null)
            {
                ShowResultErrors(previewResult);
                return;
            }

            var changes = previewResult.Payload.Changes;

            if (changes.Count == 0)
            {
                StatusMessage = "No saved searches match the pattern.";
                StatusBrush = Brushes.DarkOrange;
                return;
            }

            if (!_confirmationService.Confirm(changes))
            {
                StatusMessage = "Replacement cancelled. No changes were made.";
                StatusBrush = Brushes.DimGray;
                return;
            }

            StatusMessage = "Applying the confirmed changes...";

            var updateCommand = previewCommand with
            {
                Preview = false,
                ConfirmedChanges = changes
            };

            var updateResult = await _dispatcher.Send(
                updateCommand,
                CancellationToken.None);

            if (!updateResult.IsSuccess || updateResult.Payload is null)
            {
                ShowResultErrors(updateResult);
                return;
            }

            StatusMessage =
                $"Update complete. Succeeded: {updateResult.Payload.SucceededCount}; " +
                $"failed: {updateResult.Payload.FailedCount}.";

            StatusBrush = updateResult.Payload.FailedCount == 0
                ? Brushes.DarkGreen
                : Brushes.DarkOrange;

            await FindMatchesAsync();
        }
        catch (RegexMatchTimeoutException)
        {
            SetError("The regex took too long to evaluate.");
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    private bool ValidateSearchInputs()
    {
        if (string.IsNullOrWhiteSpace(App))
        {
            SetError("Enter the Splunk app namespace.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Owner))
        {
            SetError("Enter the saved search owner.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(SplRegex))
        {
            SetError("Enter the SPL regular expression to find.");
            return false;
        }

        return true;
    }

    private void ShowResultErrors(BaseResult result)
    {
        var errors = result.ValidationErrors?
            .Select(error => error.Message)
            .ToArray();

        SetError(errors is { Length: > 0 }
            ? string.Join(Environment.NewLine, errors)
            : result.Message ?? "The operation failed.");
    }

    private void SetError(string message)
    {
        StatusMessage = message;
        StatusBrush = Brushes.Firebrick;
    }

    private void NotifyCommands()
    {
        FindMatchesCommand.NotifyCanExecuteChanged();
        ApplyReplacementCommand.NotifyCanExecuteChanged();
    }
}