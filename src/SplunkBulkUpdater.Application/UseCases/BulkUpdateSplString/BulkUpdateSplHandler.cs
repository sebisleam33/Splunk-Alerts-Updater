using FluentValidation;
using SplunkBulkUpdater.Application.Abstractions.Persistence;
using SplunkBulkUpdater.Application.Models.SavedSearches;
using SplunkBulkUpdater.Application.UseCases.Common;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace SplunkBulkUpdater.Application.UseCases.BulkUpdateSplString;

public class BulkUpdateSplHandler : AbstractCommandHandler<BulkUpdateSplCommand, BaseResult<BulkUpdateSplPayload>>
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
        SavedSearchQuery query = new(command.App, command.Owner);

        var searches = await _savedSearchRepository.GetAllAsync(
            query,
            cancellationToken);

        Regex regex = new(
            command.SplRegex,
            RegexOptions.None,
            TimeSpan.FromSeconds(2));

        var changes = searches
            .Where(search => regex.IsMatch(search.SplSearchString))
            .Select(search =>
            {
                string updatedSpl = regex.Replace(
                    search.SplSearchString,
                    command.Replacement);

                return new SplChangePreview(
                    search.Id,
                    search.SplSearchString,
                    updatedSpl);
            })
            .ToList();

        if (command.Preview)
        {
            var previewPayload = new BulkUpdateSplPayload(
                SucceededCount: 0,
                FailedCount: 0,
                Changes: changes);

            return BaseResult<BulkUpdateSplPayload>.Success(previewPayload);
        }

        var updates = changes
            .Select(change => new SavedSearchSearchUpdate(
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
}
