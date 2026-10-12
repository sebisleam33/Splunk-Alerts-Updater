using SplunkBulkUpdater.Application.Models.SavedSearches;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.RestClient;

internal partial class SplunkRestClient
{
    public async Task UpdateAsync(SavedSearchId id, SavedSearch newSavedSearch, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(newSavedSearch);

        var path =
            $"servicesNS/{EscapePathSegment(id.Owner)}/{EscapePathSegment(id.App)}" +
            $"/saved/searches/{EscapePathSegment(id.Name)}";

        var fields = new Dictionary<string, string>
        {
            ["search"] = newSavedSearch.SplSearchString
        };

        using var content = new FormUrlEncodedContent(fields);

        using var response = await _httpClient.PostAsync(
            path,
            content,
            cancellationToken);

        if (response.IsSuccessStatusCode)
            return;

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        throw new HttpRequestException(
            $"Splunk update failed for saved search '{id.Name}' " +
            $"in app '{id.App}' owned by '{id.Owner}'. " +
            $"Status: {(int)response.StatusCode} ({response.ReasonPhrase}). " +
            $"Response: {responseBody}",
            inner: null,
            response.StatusCode);
    }

    private static string EscapePathSegment(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return Uri.EscapeDataString(value);
    }
}
