using SplunkBulkUpdater.Application.Models.SavedSearches;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.RestClient;

internal partial class SplunkRestClient
{
    public Task<IReadOnlyList<SavedSearch>> GetAllAsync(SavedSearchQuery query, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
