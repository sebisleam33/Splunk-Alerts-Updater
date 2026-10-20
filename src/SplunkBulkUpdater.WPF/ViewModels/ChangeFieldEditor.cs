using CommunityToolkit.Mvvm.ComponentModel;

namespace SplunkBulkUpdater.WPF.ViewModels;

public partial class ChangeFieldEditor : ObservableObject
{
    public ChangeFieldEditor(string key, string label)
    {
        Key = key;
        Label = label;
    }

    public string Key { get; }
    public string Label { get; }

    [ObservableProperty]
    private bool isSelected = false;

    [ObservableProperty]
    private string matchSubstring = string.Empty;

    [ObservableProperty]
    private string replacementValue = string.Empty;
}