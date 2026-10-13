namespace SplunkBulkUpdater.Application.Abstractions.Persistence;

using SplunkBulkUpdater.Application.Models.SavedSearches;

public interface ISavedSearchRepository
{
    Task<IReadOnlyList<SavedSearch>> GetAllAsync(
        SavedSearchQuery query,
        CancellationToken cancellationToken);

    Task<BulkUpdateResult> UpdateSplStringsAsync(
        IReadOnlyCollection<SavedSearchUpdate> updates,
        CancellationToken cancellationToken);
}