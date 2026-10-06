using Microsoft.Extensions.Options;
using Splunk.Client;
using SplunkBulkUpdater.Infrastructure.Splunk.Configuration;

namespace SplunkBulkUpdater.Infrastructure.Splunk;

internal sealed class SplunkServiceFactory : ISplunkServiceFactory
{
    private readonly SplunkConnectionOptions _options;

    public SplunkServiceFactory(IOptions<SplunkConnectionOptions> options)
    {
        _options = options.Value;
    }

    public async Task<Service> CreateAuthenticatedAsync(
        string username,
        string password,
        string app,
        string owner,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(app);
        ArgumentException.ThrowIfNullOrWhiteSpace(owner);
        cancellationToken.ThrowIfCancellationRequested();

        var service = new Service(
            Scheme.Https,
            _options.Host,
            _options.Port,
            new Namespace(user: owner, app: app));

        try
        {
            await service.LogOnAsync(username, password);
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