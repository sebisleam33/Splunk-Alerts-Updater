using Microsoft.Extensions.Options;
using SplunkBulkUpdater.Application.Abstractions.Authentication;
using SplunkBulkUpdater.Infrastructure.Splunk.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.RestClient;

internal partial class SplunkRestClient : ISplunkRestClient
{
    private readonly HttpClient _httpClient;
    private readonly IAuthenticator _authenticator;

    public SplunkRestClient(
        HttpClient httpClient,
        IAuthenticator authenticator,
        IOptions<SplunkConnectionOptions> options)
    {
        _httpClient = httpClient;
        _authenticator = authenticator;

        var settings = options.Value;

        _httpClient.BaseAddress = new Uri(
            $"https://{settings.Host}:{settings.Port}/");
    }

    private async Task<JsonDocument> GetJsonAsync(
        string path,
        CancellationToken cancellationToken)
    {
        using var request = CreateRequest(
            HttpMethod.Get,
            path);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        await EnsureSuccessAsync(
            response,
            cancellationToken);

        await using var stream =
            await response.Content.ReadAsStreamAsync(
                cancellationToken);

        return await JsonDocument.ParseAsync(
            stream,
            cancellationToken: cancellationToken);
    }

    private static HttpRequestMessage CreateRequest(
    HttpMethod method,
    string path)
    {
        var request = new HttpRequestMessage(method, path);

        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        return request;
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(
            cancellationToken);

        throw new HttpRequestException(
            $"Splunk REST API returned {(int)response.StatusCode} " +
            $"({response.ReasonPhrase}). Response: {body}",
            null,
            response.StatusCode);
    }

    private static string BuildCollectionPath(
        string owner,
        string app)
    {
        return $"servicesNS/{Uri.EscapeDataString(owner)}/" +
               $"{Uri.EscapeDataString(app)}/saved/searches";
    }

    private static string ReadString(
        JsonElement element,
        string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object ||
            !element.TryGetProperty(
                propertyName,
                out var value))
        {
            return string.Empty;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String =>
                value.GetString() ?? string.Empty,

            JsonValueKind.Null =>
                string.Empty,

            _ =>
                value.ToString()
        };
    }

    private static bool IsAlert(
        JsonElement content)
    {
        var alertType =
            ReadString(content, "alert_type");

        if (!string.IsNullOrWhiteSpace(alertType) &&
            !alertType.Equals(
                "always",
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var actions =
            ReadString(content, "actions");

        return !string.IsNullOrWhiteSpace(actions);
    }
}