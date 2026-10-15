using System.Text.Json;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.RestClient;

internal partial class SplunkRestClient
{
    public async Task<IReadOnlyList<string>> GetAppsAsync(CancellationToken cancellationToken)
    {
        using var document = await GetJsonAsync(
            "services/apps/local?output_mode=json&count=0", cancellationToken);

        return document.RootElement.GetProperty("entry")
            .EnumerateArray()
            .Select(entry => entry.GetProperty("name").GetString())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
