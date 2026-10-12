using SplunkBulkUpdater.Application.Models.SavedSearches;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.RestClient
{
    internal interface ISplunkRestClient
    {
        Task<IReadOnlyList<SavedSearch>> GetAllAsync(
            SavedSearchQuery query,
            CancellationToken cancellationToken);

        Task<SavedSearch> GetAsync(
            SavedSearchId id,
            CancellationToken cancellationToken);

        Task UpdateAsync(
            SavedSearchId id,
            string newSearch, 
            CancellationToken cancellationToken);

        // UpdateActionsAsync(...)
    }
}