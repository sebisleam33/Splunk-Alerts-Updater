using FluentValidation;
using SplunkBulkUpdater.Application.Abstractions.Persistence;
using SplunkBulkUpdater.Application.Models.SavedSearches;
using SplunkBulkUpdater.Application.UseCases.Common;
using System.Text.RegularExpressions;

namespace SplunkBulkUpdater.Application.UseCases.BulkUpdateSplString;

public class BulkUpdateSplHandler : AbstractCommandHandler<BulkUpdateSplCommand, BaseResult<BulkUpdateSplPayload>>
{
    private readonly ISavedSearchRepository _savedSearchRepository;

    public BulkUpdateSplHandler(IValidator<BulkUpdateSplCommand> validator, ISavedSearchRepository savedSearchRepository) : base(validator)
    {
        _savedSearchRepository = savedSearchRepository;
    }

    protected override BaseResult<BulkUpdateSplPayload> CreateValidationFailure(IReadOnlyList<ValidationError> errors)
    {
        return BaseResult<BulkUpdateSplPayload>.FromValidationErrors(errors);
    }

    protected override async Task<BaseResult<BulkUpdateSplPayload>> ExecuteAsync(BulkUpdateSplCommand command, CancellationToken cancellationToken)
    {
        SavedSearchQuery q = new(command.App, command.Owner);

        var searches = await _savedSearchRepository.GetAllAsync(command.username, command.password, q, cancellationToken);

        Regex regex = new(command.SplRegex, RegexOptions.None, TimeSpan.FromSeconds(2));

        var updatedSearches = searches
            .Where(x => regex.IsMatch(x.SplSearchString))
            .Select(x => new SavedSearchSearchUpdate(x.Id, regex.Replace(x.SplSearchString, command.Replacement)))
            .ToList();

        var repoResult = await _savedSearchRepository.UpdateSplStringsAsync(command.username, command.password, updatedSearches, cancellationToken);
        BulkUpdateSplPayload payload = new(repoResult.SucceededCount, repoResult.FailedCount);

        return BaseResult<BulkUpdateSplPayload>.Success(payload);
    }
}
