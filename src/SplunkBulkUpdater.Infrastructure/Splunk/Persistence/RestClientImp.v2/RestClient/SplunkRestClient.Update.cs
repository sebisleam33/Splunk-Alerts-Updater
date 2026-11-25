using SplunkBulkUpdater.Application.Models.SavedSearches;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.RestClient;

internal partial class SplunkRestClient
{
    public async Task UpdateAsync(
    SavedSearchUpdate update,
    CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(update);

        var id = update.Id;
        var path =
            $"{BuildCollectionPath(id.Owner, id.App)}/" +
            Uri.EscapeDataString(id.Name);

        using var request = CreateRequest(HttpMethod.Post, path);
        request.Content = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["search"] = update.SplSearchString
            });

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }
}