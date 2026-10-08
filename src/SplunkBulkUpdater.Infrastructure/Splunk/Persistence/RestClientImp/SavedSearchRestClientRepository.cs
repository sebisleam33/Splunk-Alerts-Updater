
using SplunkBulkUpdater.Application.Abstractions.Persistence;
using SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp;

namespace SplunkBulkUpdater.Infrastructure.Persistence;

public partial class SavedSearchRestClientRepository : ISavedSearchRepository
{
    private readonly SplunkRestClient _restClient;

    public SavedSearchRestClientRepository(SplunkRestClient restClient)
    {
        _restClient = restClient;
    }
}
