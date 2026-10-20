using FluentValidation;
using Microsoft.Extensions.Logging;
using SplunkBulkUpdater.Application.Abstractions.Persistence;
using SplunkBulkUpdater.Application.Models.ChangeTypes;
using SplunkBulkUpdater.Application.Models.Paging;
using SplunkBulkUpdater.Application.Models.SavedSearches;
using SplunkBulkUpdater.Application.UseCases.Common;

namespace SplunkBulkUpdater.Application.UseCases.GetAlerts;

public class GetAlertsHandler
    : AbstractCommandHandler<
        GetAlertsCommand,
        BaseResult<PageResult<GetAlertsPayload>>>
{
    private readonly ISavedSearchRepository _savedSearchRepository;

    public GetAlertsHandler(
        IValidator<GetAlertsCommand> validator,
        ISavedSearchRepository savedSearchRepository,
        IApplicationExceptionMapper exceptionMapper,
        ILogger<GetAlertsHandler> logger)
        : base(
            validator,
            exceptionMapper,
            logger)
    {
        _savedSearchRepository = savedSearchRepository;
    }

    protected override BaseResult<PageResult<GetAlertsPayload>>
        CreateExceptionFailure(
            ApplicationError error)
    {
        return BaseResult<
            PageResult<GetAlertsPayload>>.Failure(
                error.Code,
                error.Message);
    }

    protected override BaseResult<PageResult<GetAlertsPayload>>
        CreateValidationFailure(
            IReadOnlyList<ValidationError> errors)
    {
        return BaseResult<
            PageResult<GetAlertsPayload>>.FromValidationErrors(
                errors);
    }

    protected override async Task<
        BaseResult<PageResult<GetAlertsPayload>>>
        ExecuteAsync(
            GetAlertsCommand command,
            CancellationToken cancellationToken)
    {

        IReadOnlyDictionary<string, string>? fieldFilters =
            command.ChangeType is { } changeType &&
            changeType != ChangeType.SPL
                ? new Dictionary<string, string>
                {
                    [changeType.ToSplunkActionKey()] = "1"
                }
                : null;

        var query = new SavedSearchQuery(
            command.App,
            command.Owner,
            AlertsOnly: true,
            Paging: command.Paging,
            SearchStringFilters: command.SearchStringFilters,
            FieldsFilters: fieldFilters);

        var page = await _savedSearchRepository.GetPageAsync(
            query,
            cancellationToken);

        var searches = page.Items.Select(s => new GetAlertsPayload(s)).ToList();

        return BaseResult<
            PageResult<GetAlertsPayload>>.Success(
                new PageResult<GetAlertsPayload>(
                    searches,
                    page.Page));
    }
}