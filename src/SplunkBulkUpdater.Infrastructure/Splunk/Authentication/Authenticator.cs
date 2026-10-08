using SplunkBulkUpdater.Application.Abstractions.Authentication;
using SplunkBulkUpdater.Application.Abstractions.Authentication.Exceptions;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Authentication;

public class Authenticator : IAuthenticator
{
    private readonly ISplunkServiceFactory _splunkServiceFactory;
    private Credentials? _credentials;

    public Authenticator(ISplunkServiceFactory splunkServiceFactory)
    {
        _splunkServiceFactory = splunkServiceFactory;
    }

    public async Task Authenticate(Credentials credentials)
    {
        // if this call throws, the app layer will handle exception
        await _splunkServiceFactory.CreateAuthenticatedAsync(credentials);

        _credentials = credentials;
    }

    public Credentials GetCredentials()
    {
        if (_credentials == null)
        {
            throw new AuthenticationRequiredException();
        }

        return _credentials;
    }
}
