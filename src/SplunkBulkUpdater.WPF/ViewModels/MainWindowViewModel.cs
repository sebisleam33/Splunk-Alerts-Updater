using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.Models.Paging;
using SplunkBulkUpdater.Application.UseCases.Common;
using SplunkBulkUpdater.Application.UseCases.RetrieveChangeTypes;
using SplunkBulkUpdater.WPF.Dialogs;
using System.ComponentModel;
using System.Windows.Media;

namespace SplunkBulkUpdater.WPF.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public string SignedInUsername => _signedInUser;
    private readonly ICommandDispatcher _dispatcher;
    private readonly IReplacementConfirmationService _confirmationService;
    private readonly IAlertActionConfirmationService _actionConfirmationService;
    private readonly string _signedInUser;
    public event EventHandler? LogoutRequested;

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

    [RelayCommand(CanExecute = nameof(CanLogout))]
    private void Logout()
    {
        LogoutRequested?.Invoke(this, EventArgs.Empty);
    }

    private bool CanLogout()
    {
        return !IsBusy;
    }

    private async Task FetchMatchPageAsync(int pageNumber)
    {
        if (_isLoadingMoreMatches)
            return;

        _isLoadingMoreMatches = true;
        LoadMoreMatchesCommand.NotifyCanExecuteChanged();

        try
        {
            var result = await _dispatcher.Send(
                new GetAlertsCommand(
                    FixedApp,
                    _signedInUser,
                    new PageQuery(MatchPageSize, pageNumber),
                    SelectedChangeType,
                    SearchStringFilters: SearchPatterns),
                CancellationToken.None);

            if (!result.IsSuccess || result.Payload is not { } page)
            {
                ShowResultErrors(result);
                return;
            }

            foreach (var item in page.Items)
            {
                var savedSearch = item.SavedSearch;

                var row = new SelectableSavedSearch(
                    savedSearch.Id,
                    savedSearch.SplSearchString,
                    savedSearch.ActionSettings.AvailableActions);

                row.PropertyChanged += Match_PropertyChanged;

                Matches.Add(row);
                DisplayedMatches.Add(row);
            }

            DisplayedMatchCount = DisplayedMatches.Count;
            _nextPageNumber = page.Page.PageNumber + 1;
            HasMoreMatches = !page.Page.IsLastPage;

            OnPropertyChanged(nameof(SelectedMatchCount));
        }
        finally
        {
            _isLoadingMoreMatches = false;
            LoadMoreMatchesCommand.NotifyCanExecuteChanged();
        }
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

    private void Match_PropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectableSavedSearch.IsSelected))
            OnPropertyChanged(nameof(SelectedMatchCount));
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
        LogoutCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsBusyChanged(bool value)
    {
        LogoutCommand.NotifyCanExecuteChanged();
    }
}