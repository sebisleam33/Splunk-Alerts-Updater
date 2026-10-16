using Microsoft.Extensions.Logging;
using SplunkBulkUpdater.Application.Models.Paging;
using SplunkBulkUpdater.Application.Models.SavedSearches;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.Repo;

internal sealed partial class SavedSearchRestClientRepository
{
    public async Task<PageResult<SavedSearch>> GetPageAsync(
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
            "Retrieving page {PageNumber} with size {PageSize} " +
            "for app {App} and owner {Owner}.",
            query.Paging.PageNumber,
            query.Paging.PageSize,
            query.App,
            query.Owner);

        return await _restClient.GetPageAsync(
            query,
            cancellationToken);
    }
}