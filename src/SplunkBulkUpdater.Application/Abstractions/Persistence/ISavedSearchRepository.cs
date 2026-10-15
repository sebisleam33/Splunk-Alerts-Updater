using SplunkBulkUpdater.Application.Models.ChangeTypes;
using SplunkBulkUpdater.Application.Models.Paging;
using SplunkBulkUpdater.Application.Models.SavedSearches;

namespace SplunkBulkUpdater.Application.Abstractions.Persistence
{
    public interface ISavedSearchRepository
    {
        Task<(IReadOnlyList<SavedSearch>, PageResultInfo)> GetAllAsync(
            SavedSearchQuery query,
            CancellationToken cancellationToken);

        Task<BulkUpdateResult> UpdateSplStringsAsync(
            IReadOnlyCollection<SavedSearchUpdate> updates,
            CancellationToken cancellationToken);

        Task<BulkUpdateResult> UpdateActionSettingsAsync(
            IReadOnlyCollection<SavedSearchActionUpdate> updates,
            CancellationToken cancellationToken);

        Task<ChangeTypeFields> GetChangeTypeFieldSchema(
            ChangeType changeType,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<string>> GetAppsAsync(CancellationToken cancellationToken);
    }
}
