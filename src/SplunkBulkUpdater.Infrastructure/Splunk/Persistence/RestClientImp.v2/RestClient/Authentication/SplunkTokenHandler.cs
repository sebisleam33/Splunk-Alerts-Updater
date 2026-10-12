using System.Net.Http.Headers;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.RestClient.Authentication;

internal sealed class SplunkTokenHandler : DelegatingHandler
{
    private readonly ISplunkSessionTokenProvider _tokenProvider;

    public SplunkTokenHandler(ISplunkSessionTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var sessionKey =
            await _tokenProvider.GetTokenAsync(cancellationToken);

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Splunk", sessionKey);

        return await base.SendAsync(request, cancellationToken);
    }
}