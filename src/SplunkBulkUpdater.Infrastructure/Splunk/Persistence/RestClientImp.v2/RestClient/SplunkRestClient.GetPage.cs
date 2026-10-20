using SplunkBulkUpdater.Application.Models.Paging;
using SplunkBulkUpdater.Application.Models.SavedSearches;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.RestClient;

internal partial class SplunkRestClient
{
    public async Task<PageResult<SavedSearch>> GetPageAsync(
        SavedSearchQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var path = BuildCollectionPath(query.Owner!, query.App!);

        var pageQuery = query.Paging;

        var fieldFilters = query.FieldsFilters?
            .Select(filter => new KeyValuePair<string, string>(
                filter.Key,
                filter.Value))
            .ToList() ?? [];

        if (query.SearchStringFilters is { } searchFilters &&
            searchFilters.Any())
        {
            fieldFilters.Add(BuildSearchParameter(searchFilters));
        }

        fieldFilters.Add(new("output_mode", "json"));

        var url = AddQueryParameters(path, fieldFilters);
        url = AddPaging(url, query.Paging);

        var searches = new List<SavedSearch>();

        using var document = await GetJsonAsync(url, cancellationToken);
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
            pageQuery?.PageSize ?? returnedCount,
            pageQuery?.PageNumber ?? 0,
            returnedCount,
            totalCount);

        return new PageResult<SavedSearch>(
            searches,
            pageResultInfo);
    }

    private static string AddPaging(
    string path,
    PageQuery? paging)
    {
        if (paging is null)
        {
            return path;
        }

        return AddQueryParameters(path, new Dictionary<string, string>
        {
            ["count"] = paging.PageSize.ToString(
                System.Globalization.CultureInfo.InvariantCulture),
            ["offset"] = paging.Offset.ToString(
                System.Globalization.CultureInfo.InvariantCulture)
        });
    }

    private static KeyValuePair<string, string> BuildSearchParameter(
    IEnumerable<KeyValuePair<string, string>> filters)
    {
        ArgumentNullException.ThrowIfNull(filters);

        var conditions = string.Join(
            " ",
            filters.Select(filter => $"{filter.Key}={filter.Value}"));

        return new KeyValuePair<string, string>(
            "search",
            $"search=*{conditions}*");
    }

    private static string AddQueryParameters(
        string path,
        IEnumerable<KeyValuePair<string, string>> parameters)
    {
        var separator = path.Contains('?') ? '&' : '?';

        return path + separator + string.Join(
            "&",
            parameters.Select(parameter =>
                $"{Uri.EscapeDataString(parameter.Key)}=" +
                Uri.EscapeDataString(parameter.Value)));
    }
}