using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SplunkBulkUpdater.Application.Models.ChangeTypes;
using SplunkBulkUpdater.Application.Models.SavedSearches;
using System.Collections.ObjectModel;

namespace SplunkBulkUpdater.WPF.ViewModels;

public partial class MainWindowViewModel
{
    public const int MatchPageSize = 20;

    [ObservableProperty]
    private bool hasMoreMatches;

    private int _nextPageNumber;
    private bool _isLoadingMoreMatches;

    private bool CanLoadMoreMatches() =>
        HasMoreMatches && !_isLoadingMoreMatches && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanLoadMoreMatches))]
    private async Task LoadMoreMatchesAsync()
    {
        await FetchMatchPageAsync(_nextPageNumber);
    }

    [ObservableProperty]
    private SelectableSavedSearch? selectedMatch;

    [ObservableProperty]
    private int displayedMatchCount;

    public ObservableCollection<SelectableSavedSearch> Matches { get; } = [];

    public ObservableCollection<SelectableSavedSearch> DisplayedMatches { get; } = [];

    public int SelectedMatchCount => Matches.Count(item => item.IsSelected);
}

public sealed record OwnerFilterOption(string Label, string Owner)
{
    public override string ToString() => Label;
}

public partial class SelectableSavedSearch : ObservableObject
{
    public SelectableSavedSearch(
        SavedSearchId id,
        string splSearchString,
        IReadOnlyList<ChangeType> availableActions)
    {
        Id = id;
        SplSearchString = splSearchString;
        AvailableActions = availableActions;
    }

    public SavedSearchId Id { get; }

    public string SplSearchString { get; }

    public IReadOnlyList<ChangeType> AvailableActions { get; }


    public string AvailableActionsText =>
        AvailableActions.Count == 0
            ? "None"
            : string.Join(", ", AvailableActions);

    [ObservableProperty]
    private bool isSelected = true;
}