using FluentValidation;
using Microsoft.Extensions.Logging;
using SplunkBulkUpdater.Application.Abstractions.Persistence;
using SplunkBulkUpdater.Application.Models.ChangeTypes;
using SplunkBulkUpdater.Application.Models.SavedSearches;
using SplunkBulkUpdater.Application.UseCases.Common;
using System.Text.RegularExpressions;

namespace SplunkBulkUpdater.Application.UseCases.GetAlerts;

public sealed class GetAlertsHandler
    : AbstractCommandHandler<
        GetAlertsCommand,
        BaseResult<IReadOnlyList<GetAlertsPayload>>>
{
    private readonly ISavedSearchRepository _savedSearchRepository;

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
        CreateExceptionFailure(ApplicationError error)
    {
        return BaseResult<IReadOnlyList<GetAlertsPayload>>.Failure(
            error.Code,
            error.Message);
    }

    protected override BaseResult<IReadOnlyList<GetAlertsPayload>>
        CreateValidationFailure(IReadOnlyList<ValidationError> errors)
    {
        return BaseResult<IReadOnlyList<GetAlertsPayload>>
            .FromValidationErrors(errors);
    }

    protected override async Task<BaseResult<IReadOnlyList<GetAlertsPayload>>>
        ExecuteAsync(
            GetAlertsCommand command,
            CancellationToken cancellationToken)
    {
        var query = new SavedSearchQuery(
            command.App,
            command.Owner,
            AlertsOnly: true);

        var searches = await _savedSearchRepository.GetAllAsync(
            query,
            cancellationToken);

        var regex = new Regex(
            command.SplRegex,
            RegexOptions.None,
            TimeSpan.FromSeconds(2));

        var matches = searches
            .Where(search => regex.IsMatch(search.SplSearchString))
            .Where(search =>
                command.ChangeType is null ||
                command.ChangeType == ChangeType.SPL ||
                HasChangeType(search, command.ChangeType.Value))
            .Select(search => new GetAlertsPayload(
                search.Id,
                search.SplSearchString,
                search.ActionSettings.AvailableActions))
            .ToList();

        return BaseResult<IReadOnlyList<GetAlertsPayload>>
            .Success(matches);
    }

    private static bool HasChangeType(
        SavedSearch search,
        ChangeType changeType)
    {
        return changeType switch
        {
            ChangeType.Email =>
                search.ActionSettings.Values.TryGetValue(
                    "action.email",
                    out var emailEnabled) &&
                emailEnabled == "1",

            ChangeType.Webhook =>
                search.ActionSettings.Values.Any(item =>
                    item.Key.StartsWith(
                        "action.webhook.",
                        StringComparison.Ordinal)) &&
                (!search.ActionSettings.Values.TryGetValue(
                    "action.webhook",
                    out var webhookEnabled) ||
                 webhookEnabled == "1"),

            _ => true
        };
    }
}