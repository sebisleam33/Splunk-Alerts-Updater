using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SplunkBulkUpdater.Application.Models.Paging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace SplunkBulkUpdater.WPF.ViewModels;

public partial class MainWindowViewModel
{
    private const string FixedApp = "search";

    private readonly ObservableCollection<RegexSearchInput>
        _additionalRegexInputs = [];

    private ReadOnlyObservableCollection<RegexSearchInput>?
        _readOnlyAdditionalRegexInputs;

    [ObservableProperty]
    private string splRegex = string.Empty;

    [ObservableProperty]
    private string? selectedApp = FixedApp;

    [ObservableProperty]
    private OwnerFilterOption? selectedOwnerFilter;

    public ObservableCollection<string> Apps { get; } = [];

    public ObservableCollection<OwnerFilterOption> OwnerFilters { get; } = [];

    public ReadOnlyObservableCollection<RegexSearchInput> AdditionalRegexInputs =>
        _readOnlyAdditionalRegexInputs ??=
            new ReadOnlyObservableCollection<RegexSearchInput>(
                _additionalRegexInputs);

    public IReadOnlyList<string> SearchPatterns =>
        new[] { SplRegex }
            .Concat(_additionalRegexInputs.Select(input => input.Pattern))
            .Where(pattern => !string.IsNullOrWhiteSpace(pattern))
            .ToArray();

    partial void OnSelectedAppChanged(string? value)
        => NotifyCommands();

    partial void OnSelectedOwnerFilterChanged(OwnerFilterOption? value)
        => NotifyCommands();

    partial void OnSplRegexChanged(string value)
    {
        OnPropertyChanged(nameof(SearchPatterns));
        NotifyCommands();
    }

    [RelayCommand]
    private void AddRegexInput()
    {
        var input = new RegexSearchInput();

        input.PropertyChanged += AdditionalRegexInput_PropertyChanged;

        _additionalRegexInputs.Add(input);

        OnPropertyChanged(nameof(SearchPatterns));
    }

    [RelayCommand]
    private void RemoveRegexInput(RegexSearchInput? input)
    {
        if (input is null || !_additionalRegexInputs.Remove(input))
            return;

        input.PropertyChanged -= AdditionalRegexInput_PropertyChanged;

        OnPropertyChanged(nameof(SearchPatterns));
        NotifyCommands();
    }

    private void AdditionalRegexInput_PropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(RegexSearchInput.Pattern))
            return;

        OnPropertyChanged(nameof(SearchPatterns));
        NotifyCommands();
    }

    [RelayCommand]
    private async Task FindMatchesAsync()
    {
        if (!ValidateSearchInputs())
            return;

        // Capture the input values for this request.
        var patterns = SearchPatterns;

        IsBusy = true;
        StatusMessage = "Retrieving matching alerts from Splunk...";
        StatusBrush = Brushes.DimGray;

        Matches.Clear();
        DisplayedMatches.Clear();
        DisplayedMatchCount = 0;
        SelectedMatch = null;

        OnPropertyChanged(nameof(SelectedMatchCount));
        OnPropertyChanged(nameof(HasMoreMatches));

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
            SetError("A search regex took too long to evaluate.");
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
            SetError("Enter the first SPL regular expression.");
            return false;
        }

        return true;
    }
}