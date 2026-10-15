using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.UseCases.Common;
using SplunkBulkUpdater.Application.UseCases.LogIn;
using System.Windows.Media;

namespace SplunkBulkUpdater.WPF.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly ICommandDispatcher _dispatcher;
    private readonly SignedInUserContext _userContext;

    public LoginViewModel(ICommandDispatcher dispatcher, SignedInUserContext userContext)
    {
        _dispatcher = dispatcher;
        _userContext = userContext;
    }

    public event EventHandler? LoginSucceeded;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private Brush statusBrush = Brushes.Firebrick;

    [RelayCommand]
    private async Task SignInAsync()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            ShowError("Enter your Splunk username.");
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ShowError("Enter your Splunk password.");
            return;
        }

        IsBusy = true;
        StatusMessage = "Signing in...";
        StatusBrush = Brushes.DimGray;

        try
        {
            var result = await _dispatcher.Send(
                new LoginCommand(Username.Trim(), Password),
                CancellationToken.None);

            if (!result.IsSuccess)
            {
                ShowResultErrors(result);
                return;
            }

            _userContext.SetUsername(Username);
            Password = string.Empty;
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            ShowError($"Sign-in failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ShowResultErrors(BaseResult result)
    {
        var messages = result.ValidationErrors?
            .Select(error => error.Message)
            .ToArray();

        ShowError(messages is { Length: > 0 }
            ? string.Join(Environment.NewLine, messages)
            : result.Message ?? "Sign-in failed.");
    }

    private void ShowError(string message)
    {
        StatusMessage = message;
        StatusBrush = Brushes.Firebrick;
    }
}
