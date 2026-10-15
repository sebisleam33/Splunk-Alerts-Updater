using System.Text.Json;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.RestClient;

internal partial class SplunkRestClient
{
    public async Task<IReadOnlyList<string>> GetSearchSkeleton(
    string actionType,
    CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actionType);

        var query = new Dictionary<string, string>
        {
            ["search"] = $"action.{actionType}=1",
            ["count"] = "1",
            ["listDefaultActionArgs"] = "true",
            ["output_mode"] = "json"
        };

        var queryString = string.Join(
            "&",
            query.Select(parameter =>
                $"{Uri.EscapeDataString(parameter.Key)}=" +
                $"{Uri.EscapeDataString(parameter.Value)}"));

        var path = $"{BuildCollectionPath("-", "-")}?{queryString}";

        using var document = await GetJsonAsync(path, cancellationToken);

        var root = document.RootElement;

        if (!root.TryGetProperty("entry", out var entries) ||
            entries.ValueKind != JsonValueKind.Array ||
            entries.GetArrayLength() == 0)
        {
            return Array.Empty<string>();
        }

        var firstEntry = entries[0];

        if (!firstEntry.TryGetProperty("content", out var content) ||
            content.ValueKind != JsonValueKind.Object)
        {
            return Array.Empty<string>();
        }

        return content
            .EnumerateObject()
            .Select(property => property.Name)
            .ToArray();
    }
}
