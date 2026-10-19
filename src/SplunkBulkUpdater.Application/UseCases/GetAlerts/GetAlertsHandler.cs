using FluentValidation;
using Microsoft.Extensions.Logging;
using SplunkBulkUpdater.Application.Abstractions.Persistence;
using SplunkBulkUpdater.Application.Models.ChangeTypes;
using SplunkBulkUpdater.Application.Models.Paging;
using SplunkBulkUpdater.Application.Models.SavedSearches;
using SplunkBulkUpdater.Application.UseCases.Common;
using System.Text.RegularExpressions;

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
        : base(validator, exceptionMapper, logger)
    {
        _savedSearchRepository = savedSearchRepository;
    }

    protected override BaseResult<PageResult<GetAlertsPayload>>
        CreateExceptionFailure(ApplicationError error)
    {
        return BaseResult<PageResult<GetAlertsPayload>>.Failure(
            error.Code,
            error.Message);
    }

    protected override BaseResult<PageResult<GetAlertsPayload>>
        CreateValidationFailure(IReadOnlyList<ValidationError> errors)
    {
        return BaseResult<PageResult<GetAlertsPayload>>
            .FromValidationErrors(errors);
    }

    protected override async Task<BaseResult<PageResult<GetAlertsPayload>>>
        ExecuteAsync(
            GetAlertsCommand command,
            CancellationToken cancellationToken)
    {
        var patterns = new[] { command.SplRegex }
            .Concat(command.AdditionalSplRegexes ?? Array.Empty<string>())
            .Distinct(StringComparer.Ordinal)
            .Select(pattern => new Regex(
                pattern,
                RegexOptions.None,
                TimeSpan.FromSeconds(2)))
            .ToArray();

        var query = new SavedSearchQuery(
            command.App,
            command.Owner,
            AlertsOnly: true,
            Paging: command.Paging ?? new PageQuery());

        var page = await _savedSearchRepository.GetPageAsync(
            query,
            cancellationToken);

        var matchedSearches = new List<GetAlertsPayload>();

        foreach (var search in page.Items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (command.ChangeType is { } changeType &&
                changeType != ChangeType.SPL &&
                !HasChangeType(search, changeType))
            {
                continue;
            }

            if (patterns.Any(regex => regex.IsMatch(search.SplSearchString)))
            {
                matchedSearches.Add(new GetAlertsPayload(search));
            }
        }

        return BaseResult<PageResult<GetAlertsPayload>>.Success(
            new PageResult<GetAlertsPayload>(
                matchedSearches,
                page.Page));
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