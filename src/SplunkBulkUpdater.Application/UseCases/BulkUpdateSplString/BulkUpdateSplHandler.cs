using FluentValidation;
using Microsoft.Extensions.Logging;
using SplunkBulkUpdater.Application.Abstractions.Persistence;
using SplunkBulkUpdater.Application.Models.SavedSearches;
using SplunkBulkUpdater.Application.UseCases.Common;

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
        : base(
            validator,
            exceptionMapper,
            logger)
    {
        _savedSearchRepository = savedSearchRepository;
    }

    protected override BaseResult<BulkUpdateSplPayload>
        CreateExceptionFailure(
            ApplicationError error) =>
        BaseResult<BulkUpdateSplPayload>.Failure(
            error.Code,
            error.Message);

    protected override BaseResult<BulkUpdateSplPayload>
        CreateValidationFailure(
            IReadOnlyList<ValidationError> errors) =>
        BaseResult<BulkUpdateSplPayload>.FromValidationErrors(
            errors);

    protected override async Task<
        BaseResult<BulkUpdateSplPayload>>
        ExecuteAsync(
            BulkUpdateSplCommand command,
            CancellationToken cancellationToken)
    {
        List<SplChangePreview> changes;

        if (!command.Preview &&
            command.ConfirmedChanges is not null)
        {
            changes = [.. command.ConfirmedChanges];
        }
        else
        {
            var page =
                await _savedSearchRepository.GetPageAsync(
                new SavedSearchQuery(
                    command.App,
                    command.Owner,
                    AlertsOnly: true,
                    SearchStringFilters: command.SearchStringFilters),
                cancellationToken);

            var searches = page.Items;
            var selectedSearchIDs = command.SelectedSearches.ToHashSet();


            changes = searches
                .Where(search =>
                    selectedSearchIDs.Contains(search.Id))
                .Where(search =>
                    search.SplSearchString.Contains(command.ReplacementPair.Substring))
                .Select(search =>
                    new SplChangePreview(
                        search.Id,
                        search.SplSearchString,
                        search.SplSearchString.Replace(
                            command.ReplacementPair.Substring,
                            command.ReplacementPair.Replacement,
                            StringComparison.OrdinalIgnoreCase)))
                .Where(change =>
                    !string.Equals(
                        change.OriginalSpl,
                        change.UpdatedSpl,
                        StringComparison.Ordinal))
                .ToList();
        }

        if (command.Preview)
        {
            return BaseResult<BulkUpdateSplPayload>.Success(
                new BulkUpdateSplPayload(
                    0,
                    0,
                    changes));
        }

        var updates = changes
            .Select(change =>
                new SavedSearchUpdate(
                    change.Id,
                    change.UpdatedSpl))
            .ToList();

        var result =
            await _savedSearchRepository
                .UpdateSplStringsAsync(
                    updates,
                    cancellationToken);

        return BaseResult<BulkUpdateSplPayload>.Success(
            new BulkUpdateSplPayload(
                result.SucceededCount,
                result.FailedCount,
                changes));
    }
}