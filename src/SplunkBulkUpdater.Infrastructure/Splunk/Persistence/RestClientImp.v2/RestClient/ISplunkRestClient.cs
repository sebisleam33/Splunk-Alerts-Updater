using SplunkBulkUpdater.Application.Models.SavedSearches;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.RestClient
{
    public interface ISplunkRestClient
    {
        Task<IReadOnlyList<SavedSearch>> GetAllAsync(
            SavedSearchQuery query,
            CancellationToken cancellationToken);

        Task<SavedSearch> GetAsync(
            SavedSearchId id,
            CancellationToken cancellationToken);

        Task UpdateAsync(
            SavedSearchUpdate update,
            CancellationToken cancellationToken);

        Task UpdateActionSettingsAsync(
            SavedSearchActionUpdate update,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<string>> GetSearchSkeleton(
            string actionType,
            CancellationToken cancellationToken);
    }
}