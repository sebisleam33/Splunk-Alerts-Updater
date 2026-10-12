using Microsoft.Extensions.Options;
using SplunkBulkUpdater.Application.Abstractions.Authentication;
using SplunkBulkUpdater.Application.Models.SavedSearches;
using SplunkBulkUpdater.Infrastructure.Splunk.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp;

public sealed class SplunkRestClient
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

    public async Task<IReadOnlyList<SavedSearch>> GetAllAsync(
        SavedSearchQuery query,
        CancellationToken cancellationToken)
    {
        var path = BuildCollectionPath(
            query.Owner!,
            query.App!);

        var searches = new List<SavedSearch>();
        const int pageSize = 100;
        var offset = 0;

        while (true)
        {
            using var document = await GetJsonAsync(
                $"{path}?output_mode=json&count={pageSize}&offset={offset}",
                cancellationToken);

            var root = document.RootElement;
            var entries = root.GetProperty("entry");

            foreach (var entry in entries.EnumerateArray())
            {
                var name = entry.GetProperty("name").GetString()!;

                var content = entry.GetProperty("content");

                if (query.AlertsOnly && !IsAlert(content))
                    continue;

                var search = ReadString(content, "search");

                var acl = entry.TryGetProperty("acl", out var aclValue)
                    ? aclValue
                    : default;

                var app = ReadString(acl, "app");
                var owner = ReadString(acl, "owner");

                searches.Add(new SavedSearch(
                    new SavedSearchId(
                        name,
                        string.IsNullOrWhiteSpace(app) ? query.App! : app,
                        string.IsNullOrWhiteSpace(owner) ? query.Owner! : owner),
                    search));
            }

            offset += entries.GetArrayLength();

            var total = root.GetProperty("paging")
                .GetProperty("total").GetInt32();

            if (offset >= total || entries.GetArrayLength() == 0)
                break;
        }

        return searches;
    }

    public async Task<SavedSearch> GetAsync(
        SavedSearchId id,
        CancellationToken cancellationToken)
    {
        var path = BuildEntityPath(id);

        using var document = await GetJsonAsync(
            $"{path}?output_mode=json",
            cancellationToken);

        var entries = document.RootElement.GetProperty("entry");

        if (entries.GetArrayLength() == 0)
            throw new KeyNotFoundException(
                $"Saved search '{id.Name}' was not found.");

        var entry = entries[0];
        var content = entry.GetProperty("content");

        return new SavedSearch(
            id,
            ReadString(content, "search"));
    }

    public async Task UpdateAsync(
        SavedSearchId id,
        string newSearch,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(newSearch);

        var path = BuildEntityPath(id);

        using var request = CreateRequest(
            HttpMethod.Post,
            $"{path}?output_mode=json");

        request.Content = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["search"] = newSearch
            });

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    private async Task<JsonDocument> GetJsonAsync(
        string path,
        CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Get, path);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        await using var stream = await response.Content
            .ReadAsStreamAsync(cancellationToken);

        return await JsonDocument.ParseAsync(
            stream,
            cancellationToken: cancellationToken);
    }

    private HttpRequestMessage CreateRequest(
        HttpMethod method,
        string path)
    {
        var credentials = _authenticator.GetCredentials();

        var token = Convert.ToBase64String(
            Encoding.UTF8.GetBytes(
                $"{credentials.Username}:{credentials.Password}"));

        var request = new HttpRequestMessage(method, path);

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Basic", token);

        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue(
                "application/json"));

        return request;
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content
            .ReadAsStringAsync(cancellationToken);

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

    private static string BuildEntityPath(SavedSearchId id)
    {
        return $"{BuildCollectionPath(id.Owner, id.App)}/" +
               Uri.EscapeDataString(id.Name);
    }

    private static string ReadString(
        JsonElement element,
        string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object ||
            !element.TryGetProperty(propertyName, out var value))
            return string.Empty;

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.Null => string.Empty,
            _ => value.ToString()
        };
    }

    private static bool IsAlert(JsonElement content)
    {
        var alertType = ReadString(content, "alert_type");

        if (!string.IsNullOrWhiteSpace(alertType) &&
            !alertType.Equals("always", StringComparison.OrdinalIgnoreCase))
            return true;

        var actions = ReadString(content, "actions");

        return !string.IsNullOrWhiteSpace(actions);
    }
}
