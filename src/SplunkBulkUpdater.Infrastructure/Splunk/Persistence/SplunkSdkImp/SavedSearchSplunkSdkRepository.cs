using SplunkBulkUpdater.Application.Abstractions.Authentication;
using SplunkBulkUpdater.Application.Abstractions.Persistence;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.SplunkSdkImp;

public partial class SavedSearchSplunkSdkRepository : ISavedSearchRepository
{
    private readonly ISplunkServiceFactory _splunkServiceFactory;
    private readonly IAuthenticator _authenticator;

    public SavedSearchSplunkSdkRepository(ISplunkServiceFactory splunkServiceFactory, IAuthenticator authenticator)
    {
        _splunkServiceFactory = splunkServiceFactory;
        _authenticator = authenticator;
    }
}
