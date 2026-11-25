using SplunkBulkUpdater.Application.Abstractions.Authentication;
using SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.RestClient.Authentication;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Authentication;

public sealed class Authenticator : IAuthenticator
{
    private readonly ISplunkSessionTokenProvider _tokenProvider;
    private Credentials? _credentials;

    public Authenticator(ISplunkSessionTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    public async Task Authenticate(
        Credentials credentials,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(credentials);

        // This calls Splunk's REST login endpoint and throws if login fails.
        await _tokenProvider.AuthenticateAsync(
            credentials,
            cancellationToken);

        _credentials = credentials;
    }

    public Credentials GetCredentials()
    {
        return _credentials
            ?? throw new InvalidOperationException(
                "You must sign in before accessing saved searches.");
    }

    public void SignOut()
    {
        _tokenProvider.Clear();
        _credentials = null;
    }
}