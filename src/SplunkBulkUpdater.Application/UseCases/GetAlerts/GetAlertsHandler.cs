using FluentValidation;
using Microsoft.Extensions.Logging;
using SplunkBulkUpdater.Application.Abstractions.Persistence;
using SplunkBulkUpdater.Application.Models.ChangeTypes;
using SplunkBulkUpdater.Application.Models.SavedSearches;
using SplunkBulkUpdater.Application.UseCases.Common;
using System.Text.RegularExpressions;

namespace SplunkBulkUpdater.Application.UseCases.GetAlerts;

public class GetAlertsHandler : AbstractCommandHandler<GetAlertsCommand, BaseResult<IReadOnlyList<GetAlertsPayload>>>
{
    private readonly ISavedSearchRepository _savedSearchRepository;


    protected override BaseResult<IReadOnlyList<GetAlertsPayload>>
    CreateExceptionFailure(ApplicationError error)
    {
        return BaseResult<IReadOnlyList<GetAlertsPayload>>.Failure(
            error.Code,
            error.Message);
    }

    public GetAlertsHandler(
    IValidator<GetAlertsCommand> validator,
    ISavedSearchRepository savedSearchRepository,
    IApplicationExceptionMapper exceptionMapper,
    ILogger<GetAlertsHandler> logger)
    : base(validator, exceptionMapper, logger)
    {
        _savedSearchRepository = savedSearchRepository;
    }

    protected override BaseResult<IReadOnlyList<GetAlertsPayload>>
    CreateValidationFailure(
        IReadOnlyList<ValidationError> errors)
    {
        return BaseResult<IReadOnlyList<GetAlertsPayload>>
            .FromValidationErrors(errors);
    }

    protected override async Task<BaseResult<IReadOnlyList<GetAlertsPayload>>> ExecuteAsync(GetAlertsCommand command, CancellationToken cancellationToken)
    {
        SavedSearchQuery q = new(command.App, command.Owner, true);

        (var searches, var pageResultInfo) = await _savedSearchRepository.GetAllAsync(q, cancellationToken);

        Regex regex = new(command.SplRegex, RegexOptions.None, TimeSpan.FromSeconds(2));

        var matchedSearches = searches
            .Where(x => regex.IsMatch(x.SplSearchString))
            .Where(x => command.ChangeType is null || command.ChangeType == ChangeType.SPL || HasChangeType(x, command.ChangeType.Value))
            .Select(x => new GetAlertsPayload(x.Id, x.SplSearchString))
            .ToList();

        return BaseResult<IReadOnlyList<GetAlertsPayload>>.Success(matchedSearches);
    }

    private static bool HasChangeType(SavedSearch search, ChangeType changeType) => changeType switch
    {
        ChangeType.Email => search.ActionSettings.Values.TryGetValue("action.email", out var emailEnabled) && emailEnabled == "1",
        ChangeType.Webhook => search.ActionSettings.Values.Any(item => item.Key.StartsWith("action.webhook.", StringComparison.Ordinal)) &&
                              (!search.ActionSettings.Values.TryGetValue("action.webhook", out var webhookEnabled) || webhookEnabled == "1"),
        _ => true
    };
}
