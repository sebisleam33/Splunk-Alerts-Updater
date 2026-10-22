using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    private IReadOnlyList<KeyValuePair<string, string>> _activeSearchPatterns = [];

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

        _activeSearchPatterns = SearchPatterns.ToArray();

        IsBusy = true;
        StatusMessage = "Retrieving matching alerts from Splunk...";
        StatusBrush = Brushes.DimGray;

        Matches.Clear();
        DisplayedMatches.Clear();
        DisplayedMatchCount = 0;
        SelectedMatch = null;
        _nextPageNumber = 0;
        HasMoreMatches = false;

        OnPropertyChanged(nameof(SelectedMatchCount));

        try
        {
            await FetchMatchPageAsync(_nextPageNumber);

            StatusMessage = HasMoreMatches
                ? $"Found {Matches.Count} matching alert(s). Select Show more to load the next page."
                : $"Found {Matches.Count} matching alert(s).";

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
            LoadMoreMatchesCommand.NotifyCanExecuteChanged();
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