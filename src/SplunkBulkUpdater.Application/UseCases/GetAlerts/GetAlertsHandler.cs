using FluentValidation;
using SplunkBulkUpdater.Application.Abstractions.Persistence;
using SplunkBulkUpdater.Application.Models.SavedSearches;
using SplunkBulkUpdater.Application.UseCases.Common;
using System.Text.RegularExpressions;

namespace SplunkBulkUpdater.Application.UseCases.GetAlerts;

public class GetAlertsHandler : AbstractCommandHandler<GetAlertsCommand, BaseResult<IReadOnlyList<GetAlertsPayload>>>
{
    private readonly ISavedSearchRepository _savedSearchRepository;

    public GetAlertsHandler(
        IValidator<GetAlertsCommand> validator,
        ISavedSearchRepository savedSearchRepository) : base(validator)
    {
        _savedSearchRepository = savedSearchRepository;
    }

    protected override BaseResult<IReadOnlyList<GetAlertsPayload>> CreateValidationFailure(IReadOnlyList<ValidationError> errors)
    {
        return BaseResult<IReadOnlyList<GetAlertsPayload>>.FromValidationErrors(errors);
    }

    protected override async Task<BaseResult<IReadOnlyList<GetAlertsPayload>>> ExecuteAsync(GetAlertsCommand command, CancellationToken cancellationToken)
    {
        SavedSearchQuery q = new(command.App, command.Owner, true);

        var searches = await _savedSearchRepository.GetAllAsync(q, cancellationToken);

        Regex regex = new(command.SplRegex, RegexOptions.None, TimeSpan.FromSeconds(2));

        var matchedSearches = searches
            .Where(x => regex.IsMatch(x.SplSearchString))
            .Select(x => new GetAlertsPayload(x.Id, x.SplSearchString))
            .ToList();

        return BaseResult<IReadOnlyList<GetAlertsPayload>>.Success(matchedSearches);
    }
}
