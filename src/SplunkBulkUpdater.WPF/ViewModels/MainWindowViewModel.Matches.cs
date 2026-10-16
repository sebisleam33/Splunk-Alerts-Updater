using CommunityToolkit.Mvvm.ComponentModel;
using SplunkBulkUpdater.Application.Models.ChangeTypes;
using SplunkBulkUpdater.Application.Models.SavedSearches;
using System.Collections.ObjectModel;

namespace SplunkBulkUpdater.WPF.ViewModels;

public partial class MainWindowViewModel
{
    public const int MatchPageSize = 50;

    [ObservableProperty]
    private SelectableSavedSearch? selectedMatch;

    [ObservableProperty]
    private int displayedMatchCount;

    public ObservableCollection<SelectableSavedSearch> Matches { get; } = [];

    public ObservableCollection<SelectableSavedSearch> DisplayedMatches { get; } = [];

    public int SelectedMatchCount => Matches.Count(item => item.IsSelected);

    public bool HasMoreMatches => DisplayedMatchCount < Matches.Count;
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