using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SplunkBulkUpdater.Application.UseCases.GetAlerts;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace SplunkBulkUpdater.WPF.ViewModels;

public partial class MainWindowViewModel
{
    [ObservableProperty] private string app = "search";
    [ObservableProperty] private string splRegex = string.Empty;
    [ObservableProperty] private string? selectedApp;
    [ObservableProperty] private OwnerFilterOption? selectedOwnerFilter;

    public ObservableCollection<string> Apps { get; } = [];
    public ObservableCollection<OwnerFilterOption> OwnerFilters { get; private set; } = [];

    partial void OnSelectedAppChanged(string? value) => NotifyCommands();

    partial void OnSplRegexChanged(string value)
    {
        NotifyCommands();
        RefreshSplGroups(value);
    }

    partial void OnSelectedOwnerFilterChanged(OwnerFilterOption? value) => NotifyCommands();

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

    private bool ValidateSearchInputs()
    {
        if (string.IsNullOrWhiteSpace(SelectedApp)) { SetError("Select a Splunk app."); return false; }
        if (SelectedOwnerFilter is null) { SetError("Select an owner filter."); return false; }
        if (string.IsNullOrWhiteSpace(SplRegex)) { SetError("Enter the SPL regular expression to find."); return false; }
        return true;
    }
}
