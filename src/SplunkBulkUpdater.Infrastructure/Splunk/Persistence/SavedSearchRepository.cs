
using SplunkBulkUpdater.Application.Abstractions.Persistence;
using SplunkBulkUpdater.Infrastructure.Splunk;

namespace SplunkBulkUpdater.Infrastructure.Persistence;

public partial class SavedSearchRepository : ISavedSearchRepository
{
    private readonly SplunkRestClient _restClient;

    public SavedSearchRepository(SplunkRestClient restClient)
    {
        _restClient = restClient;
    }
}
