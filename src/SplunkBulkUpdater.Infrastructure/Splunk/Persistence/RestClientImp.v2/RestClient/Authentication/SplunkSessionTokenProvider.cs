using Microsoft.Extensions.Options;
using SplunkBulkUpdater.Application.Abstractions.Authentication;
using SplunkBulkUpdater.Infrastructure.Splunk.Configuration;
using System.Xml.Linq;

internal sealed class SplunkSessionTokenProvider
    : ISplunkSessionTokenProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuthenticator _authenticator;
    private readonly TimeSpan _sessionLifetime;

    private string? _sessionKey;
    private DateTimeOffset _expiresAtUtc;

    public SplunkSessionTokenProvider(
        IHttpClientFactory httpClientFactory,
        IAuthenticator authenticator,
        IOptions<SplunkConnectionOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _authenticator = authenticator;
        _sessionLifetime = options.Value.SessionLifetime;

        if (_sessionLifetime <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "The Splunk session lifetime must be greater than zero.");
        }
    }

    public Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_sessionKey) &&
            DateTimeOffset.UtcNow < _expiresAtUtc)
        {
            return Task.FromResult(_sessionKey);
        }

        return LoginAsync(cancellationToken);
    }

    private async Task<string> LoginAsync(
        CancellationToken cancellationToken)
    {
        var credentials = _authenticator.GetCredentials();

        using var content = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["username"] = credentials.Username,
                ["password"] = credentials.Password
            });

        // This named client must not have SplunkTokenHandler attached,
        // or the login request would try to obtain a token recursively.
        var client = _httpClientFactory.CreateClient("SplunkLogin");

        using var response = await client.PostAsync(
            "services/auth/login",
            content,
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(
            cancellationToken);

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

        // Refresh a little before the configured timeout.
        var refreshBuffer = TimeSpan.FromMinutes(2);
        var cacheDuration = _sessionLifetime - refreshBuffer;

        if (cacheDuration <= TimeSpan.Zero)
            cacheDuration = _sessionLifetime;

        _expiresAtUtc = DateTimeOffset.UtcNow.Add(cacheDuration);

        return _sessionKey;
    }
}
