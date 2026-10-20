using SplunkBulkUpdater.Application.Models.Paging;
using SplunkBulkUpdater.Application.Models.SavedSearches;
using System.Globalization;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.RestClient;

internal partial class SplunkRestClient
{
    private const string OutputModeJson = "json";

    public async Task<PageResult<SavedSearch>> GetPageAsync(
        SavedSearchQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var path = BuildCollectionPath(query.Owner!, query.App!);
        var queryParameters = BuildQueryParameters(query);

        var url = AddQueryParameters(path, queryParameters);

        using var document = await GetJsonAsync(url, cancellationToken);

        var root = document.RootElement;
        var entries = root.GetProperty("entry");

        var searches = entries
            .EnumerateArray()
            .Where(entry => !query.AlertsOnly || IsAlert(entry.GetProperty("content")))
            .Select(entry => ParseSavedSearch(entry, query))
            .ToList();

        var totalCount = root
            .GetProperty("paging")
            .GetProperty("total")
            .GetInt64();

        var pageInfo = new PageResultInfo(
            query.Paging?.PageSize ?? searches.Count,
            query.Paging?.PageNumber ?? 0,
            searches.Count,
            totalCount);

        return new PageResult<SavedSearch>(searches, pageInfo);
    }

    private static List<KeyValuePair<string, string>> BuildQueryParameters(
        SavedSearchQuery query)
    {
        var parameters = new List<KeyValuePair<string, string>>();

        AddSearchStringFilters(parameters, query.SearchStringFilters);

        List<KeyValuePair<string, string>> fieldFilters = [.. query.FieldsFilters ?? []];
        if (query.AlertsOnly)
        {
            fieldFilters.Add(new("alert_type!", "always"));
        }

        AddFieldFilters(parameters, fieldFilters);

        parameters.Add(new("output_mode", OutputModeJson));



        AddPagingParameters(parameters, query.Paging);

        return parameters;
    }

    private static void AddSearchStringFilters(
        ICollection<KeyValuePair<string, string>> parameters,
        IEnumerable<KeyValuePair<string, string>>? filters)
    {
        if (filters is null)
        {
            return;
        }

        foreach (var (field, value) in filters)
        {
            parameters.Add(new(
                "search",
                $"search=*{field}={value}*"));
        }
    }

    private static void AddFieldFilters(
        ICollection<KeyValuePair<string, string>> parameters,
        IEnumerable<KeyValuePair<string, string>> filters)
    {
        if (filters is null)
        {
            return;
        }

        foreach (var (field, value) in filters)
        {
            parameters.Add(new("search", $"{field}={value}"));
        }
    }

    private static void AddPagingParameters(
        ICollection<KeyValuePair<string, string>> parameters,
        PageQuery? paging)
    {
        if (paging is null)
        {
            return;
        }

        parameters.Add(new(
            "count",
            paging.PageSize.ToString(CultureInfo.InvariantCulture)));

        parameters.Add(new(
            "offset",
            paging.Offset.ToString(CultureInfo.InvariantCulture)));
    }

    private static SavedSearch ParseSavedSearch(
        System.Text.Json.JsonElement entry,
        SavedSearchQuery query)
    {
        var name = entry
            .GetProperty("name")
            .GetString()!;

        var content = entry.GetProperty("content");
        var acl = entry.TryGetProperty("acl", out var aclValue)
            ? aclValue
            : default;

        var app = ReadString(acl, "app");
        var owner = ReadString(acl, "owner");
        var search = ReadString(content, "search");

        return new SavedSearch(
            new SavedSearchId(
                name,
                string.IsNullOrWhiteSpace(app) ? query.App! : app,
                string.IsNullOrWhiteSpace(owner) ? query.Owner! : owner),
            search)
        {
            ActionSettings = ReadActionSettings(content)
        };
    }

    private static string AddQueryParameters(
        string path,
        IEnumerable<KeyValuePair<string, string>> parameters)
    {
        var queryString = string.Join(
            "&",
            parameters.Select(parameter =>
                $"{Uri.EscapeDataString(parameter.Key)}=" +
                Uri.EscapeDataString(parameter.Value)));

        if (string.IsNullOrEmpty(queryString))
        {
            return path;
        }

        var separator = path.Contains('?') ? '&' : '?';
        return path + separator + queryString;
    }
}