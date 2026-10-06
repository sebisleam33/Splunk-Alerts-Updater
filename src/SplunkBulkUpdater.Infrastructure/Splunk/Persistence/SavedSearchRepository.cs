using SplunkBulkUpdater.Application.Abstractions.Persistence;
using SplunkBulkUpdater.Infrastructure.Splunk;

namespace SplunkBulkUpdater.Infrastructure.Persistence;

public partial class SavedSearchRepository : ISavedSearchRepository
{
    private readonly ISplunkServiceFactory _splunkServiceFactory;

    public SavedSearchRepository(ISplunkServiceFactory splunkServiceFactory)
    {
        _splunkServiceFactory = splunkServiceFactory;
    }
}
