using SplunkBulkUpdater.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SplunkBulkUpdater.WPF.Views.Sections;

public partial class MatchesSection : UserControl
{
    public MatchesSection()
    {
        InitializeComponent();
    }

    private void MatchesGrid_ScrollChanged(
        object sender,
        ScrollChangedEventArgs e)
    {
        if (e.VerticalChange <= 0)
            return;

        if (sender is not DataGrid grid ||
            grid.DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        var remaining = e.ExtentHeight - e.ViewportHeight - e.VerticalOffset;

        if (remaining <= MainWindowViewModel.MatchPageSize / 2d &&
            viewModel.HasMoreMatches)
        {
            viewModel.LoadNextMatchPage();
        }
    }
}