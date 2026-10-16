using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.UseCases.Common;
using SplunkBulkUpdater.Application.UseCases.RetrieveApps;
using SplunkBulkUpdater.Application.UseCases.RetrieveChangeTypes;
using SplunkBulkUpdater.WPF.Dialogs;
using System.Windows.Media;

namespace SplunkBulkUpdater.WPF.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly ICommandDispatcher _dispatcher;
    private readonly IReplacementConfirmationService _confirmationService;
    private readonly IAlertActionConfirmationService _actionConfirmationService;
    private readonly string _signedInUser;
    public event EventHandler? LogoutRequested;

    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string statusMessage = "Sign in succeeded. Enter search criteria.";
    [ObservableProperty] private Brush statusBrush = Brushes.DimGray;

    public MainWindowViewModel(
        ICommandDispatcher dispatcher,
        IReplacementConfirmationService confirmationService,
        IAlertActionConfirmationService actionConfirmationService,
        SignedInUserContext userContext)
    {
        _dispatcher = dispatcher;
        _confirmationService = confirmationService;
        _actionConfirmationService = actionConfirmationService;
        _signedInUser = userContext.Username;
        OwnerFilters =
        [
            new OwnerFilterOption("All owners", "-"),
            new OwnerFilterOption("Shared searches", "nobody"),
            new OwnerFilterOption("My searches", _signedInUser)
        ];
        SelectedOwnerFilter = OwnerFilters.Last();
    }

    [RelayCommand(CanExecute = nameof(CanLogout))]
    private void Logout()
    {
        LogoutRequested?.Invoke(this, EventArgs.Empty);
    }

    private bool CanLogout()
    {
        return !IsBusy;
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            var apps = await _dispatcher.Send(new RetrieveAppsCommand(), CancellationToken.None);
            if (apps.IsSuccess && apps.Payload is not null)
            {
                Apps.Clear();
                foreach (var appName in apps.Payload.Apps.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                    Apps.Add(appName);
                SelectedApp = Apps.FirstOrDefault() ?? "search";
            }
            else ShowResultErrors(apps);

            var types = await _dispatcher.Send(new RetrieveChangeTypesCommand(), CancellationToken.None);
            if (types.IsSuccess && types.Payload is not null)
            {
                ChangeTypes.Clear();
                foreach (var type in types.Payload.ChangeTypes)
                    ChangeTypes.Add(type);
                SelectedChangeType = ChangeTypes.FirstOrDefault();
            }
            else ShowResultErrors(types);
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; NotifyCommands(); }
    }

    private void ShowResultErrors(BaseResult result)
    {
        var errors = result.ValidationErrors?.Select(error => error.Message).ToArray();
        SetError(errors is { Length: > 0 } ? string.Join(Environment.NewLine, errors) : result.Message ?? "The operation failed.");
    }
    private void SetError(string message)
    {
        StatusMessage = message;
        StatusBrush = Brushes.Firebrick;
    }

    private void NotifyCommands()
    {
        FindMatchesCommand.NotifyCanExecuteChanged();
        ApplyReplacementCommand.NotifyCanExecuteChanged();
        LogoutCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsBusyChanged(bool value)
    {
        LogoutCommand.NotifyCanExecuteChanged();
    }
}
