namespace SplunkBulkUpdater.Application.Abstractions.Persistence;

using SplunkBulkUpdater.Application.Models.SavedSearches;

public interface ISavedSearchRepository
{
    Task<IReadOnlyList<SavedSearch>> GetAllAsync(
        string username,
        string password,
        SavedSearchQuery query,
        CancellationToken cancellationToken);

    Task<BulkUpdateResult> UpdateSplStringsAsync(
        string username,
        string password,
        IReadOnlyCollection<SavedSearchSearchUpdate> updates,
        CancellationToken cancellationToken);
}