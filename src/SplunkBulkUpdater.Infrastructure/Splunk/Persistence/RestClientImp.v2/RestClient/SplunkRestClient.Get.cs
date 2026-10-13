using SplunkBulkUpdater.Application.Models.SavedSearches;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.RestClient;

internal partial class SplunkRestClient
{
    public async Task<SavedSearch> GetAsync(
        SavedSearchId id,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(id);

        var path = BuildEntityPath(id);

        using var document = await GetJsonAsync(
            $"{path}?output_mode=json",
            cancellationToken);

        var entries = document.RootElement
            .GetProperty("entry");

        if (entries.GetArrayLength() == 0)
        {
            throw new KeyNotFoundException(
                $"Saved search '{id.Name}' was not found.");
        }

        var content = entries[0]
            .GetProperty("content");

        return new SavedSearch(
            id,
            ReadString(content, "search"))
        {
            ActionSettings = ReadActionSettings(content)
        };
    }
}