using Splunk.Client;
using SplunkBulkUpdater.Application.Abstractions.Authentication;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Authentication;

public sealed class Authenticator : IAuthenticator
{
    private readonly ISplunkServiceFactory _splunkServiceFactory;
    private Credentials? _credentials;

    public Authenticator(ISplunkServiceFactory splunkServiceFactory)
    {
        _splunkServiceFactory = splunkServiceFactory;
    }

    public async Task Authenticate(Credentials credentials)
    {
        ArgumentNullException.ThrowIfNull(credentials);

        using Service service =
            await _splunkServiceFactory.CreateAuthenticatedAsync(credentials);

        _credentials = credentials;
    }

    public Credentials GetCredentials()
    {
        return _credentials
            ?? throw new InvalidOperationException(
                "You must sign in before accessing saved searches.");
    }
}