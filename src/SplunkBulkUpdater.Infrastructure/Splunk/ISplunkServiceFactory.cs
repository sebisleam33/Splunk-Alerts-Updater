using Splunk.Client;
using SplunkBulkUpdater.Application.Abstractions.Authentication;

namespace SplunkBulkUpdater.Infrastructure.Splunk;

public interface ISplunkServiceFactory
{
    Task<Service> CreateAuthenticatedAsync(
        Credentials credentials,
        string? app = null,
        string? owner = null,
        CancellationToken cancellationToken = default);
}
