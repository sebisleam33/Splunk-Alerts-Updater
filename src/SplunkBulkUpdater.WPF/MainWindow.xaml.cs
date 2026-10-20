using SplunkBulkUpdater.WPF.ViewModels;
using System.Windows;

namespace SplunkBulkUpdater.WPF;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    public event EventHandler? LogoutRequested;

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        _viewModel.LogoutRequested += ViewModel_LogoutRequested;
    }

    private void ViewModel_LogoutRequested(
    object? sender,
    EventArgs e)
    {
        LogoutRequested?.Invoke(this, e);
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.MainWindowViewModel viewModel)
            await viewModel.InitializeAsync();
    }

    protected override void OnClosed(EventArgs e)
    {
        _viewModel.LogoutRequested -= ViewModel_LogoutRequested;

        base.OnClosed(e);
    }

    private void MatchesSection_Loaded(object sender, RoutedEventArgs e)
    {

    }

    private void SearchSection_Loaded(object sender, RoutedEventArgs e)
    {

    }
}
