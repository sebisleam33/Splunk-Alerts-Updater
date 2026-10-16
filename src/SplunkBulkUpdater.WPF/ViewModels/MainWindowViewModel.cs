using CommunityToolkit.Mvvm.ComponentModel;
using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.UseCases.Common;
using SplunkBulkUpdater.Application.UseCases.RetrieveChangeTypes;
using SplunkBulkUpdater.WPF.Dialogs;
using System.Windows.Media;

namespace SplunkBulkUpdater.WPF.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public string SignedInUsername => _signedInUser;
    private readonly ICommandDispatcher _dispatcher;
    private readonly IReplacementConfirmationService _confirmationService;
    private readonly IAlertActionConfirmationService _actionConfirmationService;
    private readonly string _signedInUser;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string statusMessage = "Sign in succeeded. Enter search criteria.";

    [ObservableProperty]
    private Brush statusBrush = Brushes.DimGray;

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

        SelectedApp = "search";

        OwnerFilters.Add(
            new OwnerFilterOption($"Yours ({_signedInUser})", _signedInUser));

        SelectedOwnerFilter = OwnerFilters[0];
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;

        try
        {
            var types = await _dispatcher.Send(
                new RetrieveChangeTypesCommand(),
                CancellationToken.None);

            if (types.IsSuccess && types.Payload is not null)
            {
                ChangeTypes.Clear();

                foreach (var type in types.Payload.ChangeTypes)
                    ChangeTypes.Add(type);

                SelectedChangeType = ChangeTypes.FirstOrDefault();
            }
            else
            {
                ShowResultErrors(types);
            }
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    private void ShowResultErrors(BaseResult result)
    {
        var errors = result.ValidationErrors?
            .Select(error => error.Message)
            .ToArray();

        SetError(
            errors is { Length: > 0 }
                ? string.Join(Environment.NewLine, errors)
                : result.Message ?? "The operation failed.");
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
    }
}