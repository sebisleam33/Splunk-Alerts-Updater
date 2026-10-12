using SplunkBulkUpdater.Application.Models.SavedSearches;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.RestClient;

internal partial class SplunkRestClient
{
    public Task UpdateAsync(SavedSearchId id, SavedSearch newSavedSearch, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
