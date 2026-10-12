using SplunkBulkUpdater.Application.Models.SavedSearches;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.RestClient;

internal partial class SplunkRestClient
{
    public async Task UpdateAsync(
        SavedSearchId id,
        SavedSearch newSavedSearch,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(newSavedSearch);

        var path =
            $"{BuildCollectionPath(id.Owner, id.App)}/" +
            Uri.EscapeDataString(id.Name);

        using var request = CreateRequest(HttpMethod.Post, path);

        var fields = new Dictionary<string, string>
        {
            ["search"] = newSavedSearch.SplSearchString
        };

        request.Content = new FormUrlEncodedContent(fields);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }
}