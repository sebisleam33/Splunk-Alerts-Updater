using SplunkBulkUpdater.Application.Abstractions.Authentication;
using SplunkBulkUpdater.Application.Abstractions.Persistence;
using SplunkBulkUpdater.Infrastructure.Splunk;

namespace SplunkBulkUpdater.Infrastructure.Persistence;

public partial class SavedSearchRepository : ISavedSearchRepository
{
    private readonly ISplunkServiceFactory _splunkServiceFactory;
    private readonly IAuthenticator _authenticator;

    public SavedSearchRepository(ISplunkServiceFactory splunkServiceFactory, IAuthenticator authenticator)
    {
        _splunkServiceFactory = splunkServiceFactory;
        _authenticator = authenticator;
    }
}
