using Microsoft.Extensions.Options;
using Splunk.Client;
using SplunkBulkUpdater.Application.Abstractions.Authentication;
using SplunkBulkUpdater.Infrastructure.Splunk.Configuration;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.SplunkSdkImp;

public sealed class SplunkServiceFactory : ISplunkServiceFactory
{
    private readonly SplunkConnectionOptions _options;

    public SplunkServiceFactory(IOptions<SplunkConnectionOptions> options)
    {
        _options = options.Value;
    }

    public async Task<Service> CreateAuthenticatedAsync(
        Credentials credentials,
        string? app = null,
        string? owner = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(credentials);
        cancellationToken.ThrowIfCancellationRequested();

        Service service;

        // Login does not yet have an app/owner namespace.
        if (app is null && owner is null)
        {
            service = new Service(
                Scheme.Https,
                _options.Host,
                _options.Port);
        }
        else
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(app);
            ArgumentException.ThrowIfNullOrWhiteSpace(owner);

            service = new Service(
                Scheme.Https,
                _options.Host,
                _options.Port,
                new Namespace(user: owner, app: app));
        }

        try
        {
            await service.LogOnAsync(credentials.Username, credentials.Password);
            cancellationToken.ThrowIfCancellationRequested();

            return service;
        }
        catch
        {
            service.Dispose();
            throw;
        }
    }
}