using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.Models.SavedSearches;
using SplunkBulkUpdater.Application.UseCases.BulkUpdateSplString;
using SplunkBulkUpdater.Application.UseCases.Common;
using SplunkBulkUpdater.Application.UseCases.GetAlerts;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace SplunkBulkUpdater.WPF.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly ICommandDispatcher _dispatcher;

    public MainWindowViewModel(ICommandDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

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
    private string statusMessage = "Enter Splunk credentials and search criteria.";

    [ObservableProperty]
    private Brush statusBrush = Brushes.DimGray;

    [ObservableProperty]
    private SavedSearch? selectedMatch;

    public ObservableCollection<SavedSearch> Matches { get; } = [];

    partial void OnUsernameChanged(string value) => NotifyCommands();
    partial void OnPasswordChanged(string value) => NotifyCommands();
    partial void OnAppChanged(string value) => NotifyCommands();
    partial void OnOwnerChanged(string value) => NotifyCommands();
    partial void OnSplRegexChanged(string value) => NotifyCommands();
    partial void OnReplacementChanged(string value) => NotifyCommands();

    [RelayCommand]
    private async Task FindMatchesAsync()
    {
        if (!ValidateCommonInputs())
            return;

        IsBusy = true;
        StatusMessage = "Connecting to Splunk and retrieving saved searches...";
        StatusBrush = Brushes.DimGray;
        Matches.Clear();

        try
        {
            var command = new GetAlertsCommand(
                Username.Trim(),
                Password,
                SplRegex,
                App.Trim(),
                Owner.Trim());

            var result = await _dispatcher.Send(command, CancellationToken.None);

            if (!result.IsSuccess)
            {
                ShowResultErrors(result);
                return;
            }

            foreach (var match in result.Payload ?? [])
            {
                Matches.Add(new SavedSearch(match.Id, match.SplSearchString));
            }

            StatusMessage = $"Found {Matches.Count} matching saved search(es).";
            StatusBrush = Brushes.DarkGreen;
        }
        catch (RegexMatchTimeoutException)
        {
            SetError("The search pattern took too long to evaluate.");
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
        if (!ValidateCommonInputs())
            return;

        if (string.IsNullOrWhiteSpace(Replacement))
        {
            SetError("Enter the replacement expression.");
            return;
        }

        if (Matches.Count == 0)
        {
            SetError("Find matching searches before applying a replacement.");
            return;
        }

        var confirmation = MessageBox.Show(
            $"This will update saved searches in app '{App}' owned by '{Owner}'. Continue?",
            "Confirm bulk update",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirmation != MessageBoxResult.Yes)
            return;

        IsBusy = true;
        StatusMessage = "Updating matching saved searches...";
        StatusBrush = Brushes.DimGray;

        try
        {
            var command = new BulkUpdateSplCommand(
                Username.Trim(),
                Password,
                SplRegex,
                Replacement,
                App.Trim(),
                Owner.Trim());

            var result = await _dispatcher.Send(command, CancellationToken.None);

            if (!result.IsSuccess || result.Payload is null)
            {
                ShowResultErrors(result);
                return;
            }

            StatusMessage =
                $"Update complete. Succeeded: {result.Payload.SucceededCount}; failed: {result.Payload.FailedCount}.";

            StatusBrush = result.Payload.FailedCount == 0
                ? Brushes.DarkGreen
                : Brushes.DarkOrange;

            await FindMatchesAsync();
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

    private bool ValidateCommonInputs()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            SetError("Enter your Splunk username.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            SetError("Enter your Splunk password.");
            return false;
        }

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