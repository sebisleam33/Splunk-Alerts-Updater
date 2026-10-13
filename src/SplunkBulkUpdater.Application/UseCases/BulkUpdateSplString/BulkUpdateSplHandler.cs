using FluentValidation;
using Microsoft.Extensions.Logging;
using SplunkBulkUpdater.Application.Abstractions.Persistence;
using SplunkBulkUpdater.Application.Models.SavedSearches;
using SplunkBulkUpdater.Application.UseCases.Common;
using System.Text.RegularExpressions;

namespace SplunkBulkUpdater.Application.UseCases.BulkUpdateSplString;

public sealed class BulkUpdateSplHandler
    : AbstractCommandHandler<
        BulkUpdateSplCommand,
        BaseResult<BulkUpdateSplPayload>>
{
    private readonly ISavedSearchRepository _savedSearchRepository;

    public BulkUpdateSplHandler(
        IValidator<BulkUpdateSplCommand> validator,
        ISavedSearchRepository savedSearchRepository,
        IApplicationExceptionMapper exceptionMapper,
        ILogger<BulkUpdateSplHandler> logger)
        : base(validator, exceptionMapper, logger)
    {
        _savedSearchRepository = savedSearchRepository;
    }

    protected override BaseResult<BulkUpdateSplPayload>
    CreateExceptionFailure(ApplicationError error)
    {
        return BaseResult<BulkUpdateSplPayload>.Failure(
            error.Code,
            error.Message);
    }

    protected override BaseResult<BulkUpdateSplPayload>
    CreateValidationFailure(
        IReadOnlyList<ValidationError> errors)
    {
        return BaseResult<BulkUpdateSplPayload>
            .FromValidationErrors(errors);
    }

    protected override async Task<BaseResult<BulkUpdateSplPayload>> ExecuteAsync(
        BulkUpdateSplCommand command,
        CancellationToken cancellationToken)
    {
        List<SplChangePreview> changes;

        if (!command.Preview && command.ConfirmedChanges is not null)
        {
            // Apply precisely the changes shown in the confirmation dialog.
            changes = command.ConfirmedChanges.ToList();
        }
        else
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

            var separatorIndex = command.Replacement.IndexOf('=');
            var groupName = command.Replacement[..separatorIndex].Trim();
            var newValue = command.Replacement[(separatorIndex + 1)..];

            changes = searches
                .Where(search => regex.IsMatch(search.SplSearchString))
                .Select(search => new SplChangePreview(
                    search.Id,
                    search.SplSearchString,
                    ReplaceNamedGroup(
                        regex,
                        search.SplSearchString,
                        groupName,
                        newValue)))
                .ToList();
        }

        if (command.Preview)
        {
            return BaseResult<BulkUpdateSplPayload>.Success(
                new BulkUpdateSplPayload(
                    SucceededCount: 0,
                    FailedCount: 0,
                    Changes: changes));
        }

        var updates = changes
            .Select(change => new SavedSearchUpdate(
                change.Id,
                change.UpdatedSpl))
            .ToList();

        var repositoryResult =
            await _savedSearchRepository.UpdateSplStringsAsync(
                updates,
                cancellationToken);

        var payload = new BulkUpdateSplPayload(
            repositoryResult.SucceededCount,
            repositoryResult.FailedCount,
            changes);

        return BaseResult<BulkUpdateSplPayload>.Success(payload);
    }

    private static string ReplaceNamedGroup(
    Regex regex,
    string input,
    string groupName,
    string newValue)
    {
        return regex.Replace(input, match =>
        {
            var group = match.Groups[groupName];

            // A named group can be defined in the regex but not participate
            // in every match, for example when used in an alternation.
            if (!group.Success)
                return match.Value;

            var relativeStart = group.Index - match.Index;

            return string.Concat(
                match.Value.AsSpan(0, relativeStart),
                newValue,
                match.Value.AsSpan(relativeStart + group.Length));
        });
    }
}