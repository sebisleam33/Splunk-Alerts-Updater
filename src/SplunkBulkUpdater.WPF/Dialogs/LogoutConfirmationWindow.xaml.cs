using System.Windows;

namespace SplunkBulkUpdater.WPF.Dialogs;

public partial class LogoutConfirmationWindow : Window
{
    public LogoutConfirmationWindow()
    {
        InitializeComponent();
    }

    private void Confirm_Click(
        object sender,
        RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void Cancel_Click(
        object sender,
        RoutedEventArgs e)
    {
        DialogResult = false;
    }
}