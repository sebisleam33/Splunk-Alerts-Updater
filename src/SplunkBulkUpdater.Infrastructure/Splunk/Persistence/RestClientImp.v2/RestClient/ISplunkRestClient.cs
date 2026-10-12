using SplunkBulkUpdater.Application.Models.SavedSearches;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.RestClient;

internal interface ISplunkRestClient
{
    Task<IReadOnlyList<SavedSearch>> GetAllAsync(
        SavedSearchQuery query,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        SavedSearchId id,
        SavedSearch newSavedSearch,
        CancellationToken cancellationToken);
}
