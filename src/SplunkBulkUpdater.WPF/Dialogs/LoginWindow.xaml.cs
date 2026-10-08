using SplunkBulkUpdater.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SplunkBulkUpdater.WPF.Dialogs;

public partial class LoginWindow : Window
{
    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
        viewModel.LoginSucceeded += OnLoginSucceeded;
    }

    private void PasswordInput_OnPasswordChanged(
        object sender,
        RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel viewModel
            && sender is PasswordBox passwordBox)
        {
            viewModel.Password = passwordBox.Password;
        }
    }

    private void OnLoginSucceeded(object? sender, EventArgs e)
    {
        DialogResult = true;
    }
}