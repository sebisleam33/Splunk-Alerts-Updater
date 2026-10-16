using CommunityToolkit.Mvvm.ComponentModel;
using SplunkBulkUpdater.Application.Models.SavedSearches;
using System.Collections.ObjectModel;

namespace SplunkBulkUpdater.WPF.ViewModels;

public partial class MainWindowViewModel
{
    [ObservableProperty] private SelectableSavedSearch? selectedMatch;
    public ObservableCollection<SelectableSavedSearch> Matches { get; } = [];
    public int SelectedMatchCount => Matches.Count(item => item.IsSelected);
}

public sealed record OwnerFilterOption(string Label, string Owner)
{
    public override string ToString() => Label;
}

public partial class SelectableSavedSearch : ObservableObject
{
    public SelectableSavedSearch(SavedSearchId id, string splSearchString)
    {
        Id = id;
        SplSearchString = splSearchString;
    }

    public SavedSearchId Id { get; }
    public string SplSearchString { get; }
    [ObservableProperty] private bool isSelected = true;
}
