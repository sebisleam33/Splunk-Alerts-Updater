using CommunityToolkit.Mvvm.ComponentModel;

namespace SplunkBulkUpdater.WPF.ViewModels;

public partial class SplGroupEditor : ObservableObject
{
    public SplGroupEditor(string groupName, string replacementValue)
    {
        GroupName = groupName;
        ReplacementValue = replacementValue;
    }

    public string GroupName { get; }

    [ObservableProperty]
    private string replacementValue = string.Empty;
}