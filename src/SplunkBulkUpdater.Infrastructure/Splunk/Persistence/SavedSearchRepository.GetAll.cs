using SplunkBulkUpdater.Application.Models.SavedSearches;

namespace SplunkBulkUpdater.Infrastructure.Persistence;

public partial class SavedSearchRepository
{
    public async Task<IReadOnlyList<SavedSearch>> GetAllAsync(
        SavedSearchQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (string.IsNullOrWhiteSpace(query.App))
            throw new ArgumentException(
                "An application is required.",
                nameof(query));

        if (string.IsNullOrWhiteSpace(query.Owner))
            throw new ArgumentException(
                "An owner is required.",
                nameof(query));

        return await _restClient.GetAllAsync(
            query,
            cancellationToken);
    }
}
