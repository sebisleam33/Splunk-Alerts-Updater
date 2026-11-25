using SplunkBulkUpdater.Application.Abstractions.Authentication;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.RestClient.Authentication;

public interface ISplunkSessionTokenProvider
{
    Task AuthenticateAsync(
        Credentials credentials,
        CancellationToken cancellationToken = default);
    Task<string> GetTokenAsync(CancellationToken cancellationToken);

    void Clear();
}