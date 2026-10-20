using CommunityToolkit.Mvvm.ComponentModel;

namespace SplunkBulkUpdater.WPF.ViewModels
{
    public partial class SearchStringFilterInput : ObservableObject
    {
        [ObservableProperty]
        private string value = string.Empty;
    }
}
