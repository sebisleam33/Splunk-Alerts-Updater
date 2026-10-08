using Microsoft.Extensions.Options;
using Splunk.Client;
using SplunkBulkUpdater.Application.Abstractions.Authentication;
using SplunkBulkUpdater.Infrastructure.Splunk.Configuration;

namespace SplunkBulkUpdater.Infrastructure.Splunk;

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
        //ArgumentException.ThrowIfNullOrWhiteSpace(app);
        //ArgumentException.ThrowIfNullOrWhiteSpace(owner);
        cancellationToken.ThrowIfCancellationRequested();

        var service = new Service(
            Scheme.Https,
            _options.Host,
            _options.Port,
            new Namespace(user: owner, app: app));

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