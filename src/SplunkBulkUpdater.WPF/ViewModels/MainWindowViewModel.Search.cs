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

    private readonly ObservableCollection<SearchStringFilterInput>
        _searchStringFilters = [];

    private ReadOnlyObservableCollection<SearchStringFilterInput>?
        _readOnlySearchStringFilters;

    [ObservableProperty]
    private string? selectedApp = FixedApp;

    [ObservableProperty]
    private OwnerFilterOption? selectedOwnerFilter;

    public ObservableCollection<string> Apps { get; } = [];

    public ObservableCollection<OwnerFilterOption> OwnerFilters { get; } = [];

    public ReadOnlyObservableCollection<SearchStringFilterInput> SearchStringFilterInputs
        =>
        _readOnlySearchStringFilters ??=
            new ReadOnlyObservableCollection<SearchStringFilterInput>(
                _searchStringFilters);

    public IReadOnlyList<KeyValuePair<string, string>> SearchPatterns =>
    _searchStringFilters
        .Select(filter =>
        {
            var parts = filter.Value.Split('=', 2);

            return parts.Length == 2
                ? new KeyValuePair<string, string>(
                    parts[0].Trim(),
                    parts[1].Trim())
                : default;
        })
        .Where(pair => !string.IsNullOrWhiteSpace(pair.Key))
        .ToList();

    partial void OnSelectedAppChanged(string? value)
        => NotifyCommands();

    partial void OnSelectedOwnerFilterChanged(OwnerFilterOption? value)
        => NotifyCommands();

    [RelayCommand]
    private void AddSearchStringFilterInput()
    {
        var input = new SearchStringFilterInput();
        input.PropertyChanged += AdditionalRegexInput_PropertyChanged;
        _searchStringFilters.Add(input);

        OnPropertyChanged(nameof(SearchPatterns));
    }

    [RelayCommand]
    private void RemoveRegexInput(SearchStringFilterInput? input)
    {
        if (input is null || !_searchStringFilters.Remove(input))
            return;

        input.PropertyChanged -= AdditionalRegexInput_PropertyChanged;

        OnPropertyChanged(nameof(SearchPatterns));
        NotifyCommands();
    }

    private void AdditionalRegexInput_PropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(SearchStringFilterInput.Value))
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
                    SelectedChangeType,
                    SearchStringFilters: SearchPatterns),
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
        for (var i = 0; i < _searchStringFilters.Count; i++)
        {
            var value = _searchStringFilters[i].Value;

            if (string.IsNullOrWhiteSpace(value))
            {
                SetError($"Enter a search filter in row {i + 1}.");
                return false;
            }

            var separatorIndex = value.IndexOf('=');

            if (separatorIndex <= 0 ||
                separatorIndex == value.Length - 1)
            {
                SetError(
                    $"Search filter in row {i + 1} must use the format field=value.");
                return false;
            }
        }

        return true;
    }
}