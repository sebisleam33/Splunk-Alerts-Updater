using Microsoft.Extensions.Logging;
using SplunkBulkUpdater.Application.Models.Paging;
using SplunkBulkUpdater.Application.Models.SavedSearches;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.Repo;

internal sealed partial class SavedSearchRestClientRepository
{
    public async Task<(IReadOnlyList<SavedSearch>, PageResultInfo)> GetAllAsync(
        SavedSearchQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (string.IsNullOrWhiteSpace(query.App))
        {
            throw new ArgumentException(
                "An application is required.",
                nameof(query));
        }

        if (string.IsNullOrWhiteSpace(query.Owner))
        {
            throw new ArgumentException(
                "An owner is required.",
                nameof(query));
        }

        _logger.LogDebug(
            "Retrieving saved searches for app {App} and owner {Owner}.",
            query.App,
            query.Owner);

        return await _restClient.GetAllAsync(
            query,
            cancellationToken);
    }
}