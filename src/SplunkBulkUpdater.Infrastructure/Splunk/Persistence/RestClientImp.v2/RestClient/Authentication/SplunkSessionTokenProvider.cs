using Microsoft.Extensions.Options;
using SplunkBulkUpdater.Application.Abstractions.Authentication;
using SplunkBulkUpdater.Infrastructure.Splunk.Configuration;
using SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.RestClient.Authentication;
using System.Xml.Linq;

internal sealed class SplunkSessionTokenProvider
    : ISplunkSessionTokenProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TimeSpan _sessionLifetime;

    private Credentials? _credentials;
    private string? _sessionKey;
    private DateTimeOffset _expiresAtUtc;

    public SplunkSessionTokenProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<SplunkConnectionOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _sessionLifetime = options.Value.SessionLifetime;
    }

    public async Task AuthenticateAsync(
        Credentials credentials,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(credentials);

        // Login first. Keep the new credentials only if login succeeds.
        await LoginAsync(credentials, cancellationToken);
        _credentials = credentials;
    }

    public async Task<string> GetTokenAsync(
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(_sessionKey) &&
            DateTimeOffset.UtcNow < _expiresAtUtc)
        {
            return _sessionKey;
        }

        var credentials = _credentials
            ?? throw new InvalidOperationException(
                "Authenticate before requesting a Splunk session token.");

        return await LoginAsync(credentials, cancellationToken);
    }

    private async Task<string> LoginAsync(
        Credentials credentials,
        CancellationToken cancellationToken)
    {
        using var content = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["username"] = credentials.Username,
                ["password"] = credentials.Password
            });

        var client = _httpClientFactory.CreateClient("SplunkLogin");

        using var response = await client.PostAsync(
            "services/auth/login",
            content,
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Splunk login failed with status " +
                $"{(int)response.StatusCode} ({response.ReasonPhrase}). " +
                $"Response: {body}",
                null,
                response.StatusCode);
        }

        var document = XDocument.Parse(body);
        var sessionKey = document
            .Descendants()
            .FirstOrDefault(element => element.Name.LocalName == "sessionKey")
            ?.Value;

        if (string.IsNullOrWhiteSpace(sessionKey))
        {
            throw new InvalidOperationException(
                "Splunk login succeeded but returned no session key.");
        }

        _sessionKey = sessionKey;
        _expiresAtUtc = DateTimeOffset.UtcNow.Add(_sessionLifetime);

        return sessionKey;
    }
}