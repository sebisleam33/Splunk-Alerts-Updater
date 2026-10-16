using SplunkBulkUpdater.Application.Models.Paging;
using SplunkBulkUpdater.Application.Models.SavedSearches;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.RestClient;

internal partial class SplunkRestClient
{
    public async Task<(IReadOnlyList<SavedSearch>, PageResultInfo)> GetAllAsync(
        SavedSearchQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var path = BuildCollectionPath(
            query.Owner!,
            query.App!);

        var pageQuery = query.Paging;

        var searches = new List<SavedSearch>();

        using var document = await GetJsonAsync(
            $"{path}" +
            $"?output_mode=json" +
            $"&count={pageQuery.PageSize}" +
            $"&offset={pageQuery.Offset}",
            cancellationToken);

        var root = document.RootElement;
        var entries = root.GetProperty("entry");

        foreach (var entry in entries.EnumerateArray())
        {
            var name = entry
                .GetProperty("name")
                .GetString()!;

            var content = entry.GetProperty("content");

            if (query.AlertsOnly && !IsAlert(content))
            {
                continue;
            }

            var search = ReadString(
                content,
                "search");

            var acl = entry.TryGetProperty(
                "acl",
                out var aclValue)
                ? aclValue
                : default;

            var app = ReadString(acl, "app");
            var owner = ReadString(acl, "owner");

            searches.Add(new SavedSearch(
                new SavedSearchId(
                    name,
                    string.IsNullOrWhiteSpace(app)
                        ? query.App!
                        : app,
                    string.IsNullOrWhiteSpace(owner)
                        ? query.Owner!
                        : owner),
                search)
            {
                ActionSettings = ReadActionSettings(content)
            });
        }

        var returnedCount = entries.GetArrayLength();

        var totalCount = root
            .GetProperty("paging")
            .GetProperty("total")
            .GetInt64();

        var pageResultInfo = new PageResultInfo(
            pageQuery.PageSize,
            pageQuery.PageNumber,
            returnedCount,
            totalCount);

        return (searches, pageResultInfo);
    }
}