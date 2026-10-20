using CommunityToolkit.Mvvm.ComponentModel;

namespace SplunkBulkUpdater.WPF.ViewModels
{
    public partial class RegexSearchInput : ObservableObject
    {
        [ObservableProperty]
        private string pattern = string.Empty;
    }
}
