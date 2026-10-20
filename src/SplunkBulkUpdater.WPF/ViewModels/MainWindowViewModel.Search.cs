using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SplunkBulkUpdater.Application.Models.Paging;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace SplunkBulkUpdater.WPF.ViewModels;

public partial class MainWindowViewModel
{
    private const string FixedApp = "search";

    [ObservableProperty]
    private string splRegex = string.Empty;

    [ObservableProperty]
    private string? selectedApp = FixedApp;

    [ObservableProperty]
    private OwnerFilterOption? selectedOwnerFilter;

    public ObservableCollection<string> Apps { get; } = [];

    public ObservableCollection<OwnerFilterOption> OwnerFilters { get; } = [];

    partial void OnSelectedAppChanged(string? value) => NotifyCommands();

    partial void OnSplRegexChanged(string value)
    {
        NotifyCommands();
    }

    partial void OnSelectedOwnerFilterChanged(OwnerFilterOption? value)
        => NotifyCommands();

    [RelayCommand]
    private async Task FindMatchesAsync()
    {
        if (!ValidateSearchInputs())
            return;

        IsBusy = true;
        StatusMessage = "Retrieving matching alerts from Splunk...";
        StatusBrush = Brushes.DimGray;

        Matches.Clear();
        DisplayedMatches.Clear();
        DisplayedMatchCount = 0;

        try
        {
            var result = await _dispatcher.Send(
                new GetAlertsCommand(
                    FixedApp,
                    _signedInUser,
                    new PageQuery(),
                    SelectedChangeType),
                CancellationToken.None);
            if (!result.IsSuccess || result.Payload is not { } page)
            {
                ShowResultErrors(result);
                return;
            }

            foreach (var match in page.Items)
            {
                var savedSearch = match.SavedSearch;

                var row = new SelectableSavedSearch(
                    savedSearch.Id,
                    savedSearch.SplSearchString,
                    savedSearch.ActionSettings.AvailableActions);

                row.PropertyChanged += (_, args) =>
                {
                    if (args.PropertyName ==
                        nameof(SelectableSavedSearch.IsSelected))
                    {
                        OnPropertyChanged(nameof(SelectedMatchCount));
                    }
                };

                Matches.Add(row);
            }

            LoadNextMatchPage();

            OnPropertyChanged(nameof(SelectedMatchCount));

            StatusMessage =
                $"Found {Matches.Count} matching alert(s). " +
                "Scroll through the list to load more results.";
            StatusBrush = Brushes.DarkGreen;
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

    public void LoadNextMatchPage()
    {
        var nextPageEnd = Math.Min(
            DisplayedMatchCount + MatchPageSize,
            Matches.Count);

        while (DisplayedMatchCount < nextPageEnd)
        {
            DisplayedMatches.Add(Matches[DisplayedMatchCount]);
            DisplayedMatchCount++;
        }

        OnPropertyChanged(nameof(HasMoreMatches));
    }

    private bool ValidateSearchInputs()
    {
        if (string.IsNullOrWhiteSpace(SplRegex))
        {
            SetError("Enter the SPL regular expression to find.");
            return false;
        }

        return true;
    }
}