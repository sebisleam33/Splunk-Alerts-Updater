using Splunk.Client;

namespace SplunkBulkUpdater.Infrastructure.Splunk;

public interface ISplunkServiceFactory
{
    Task<Service> CreateAuthenticatedAsync(
        string username,
        string password,
        string app,
        string owner,
        CancellationToken cancellationToken);
}
