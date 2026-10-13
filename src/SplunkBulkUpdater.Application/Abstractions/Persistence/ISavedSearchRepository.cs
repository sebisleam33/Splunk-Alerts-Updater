using SplunkBulkUpdater.Application.Models.SavedSearches;

namespace SplunkBulkUpdater.Application.Abstractions.Persistence
{
    public interface ISavedSearchRepository
    {
        Task<IReadOnlyList<SavedSearch>> GetAllAsync(
            SavedSearchQuery query,
            CancellationToken cancellationToken);

        Task<BulkUpdateResult> UpdateSplStringsAsync(
            IReadOnlyCollection<SavedSearchUpdate> updates,
            CancellationToken cancellationToken);

        Task<BulkUpdateResult> UpdateActionSettingsAsync(
            IReadOnlyCollection<SavedSearchActionUpdate> updates,
            CancellationToken cancellationToken);
    }
}