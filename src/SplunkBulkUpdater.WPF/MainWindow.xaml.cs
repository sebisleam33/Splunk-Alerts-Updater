using SplunkBulkUpdater.WPF.ViewModels;
using System.Windows;

namespace SplunkBulkUpdater.WPF;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}