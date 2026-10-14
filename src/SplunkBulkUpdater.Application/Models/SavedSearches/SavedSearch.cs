namespace SplunkBulkUpdater.Application.Models.SavedSearches;

public sealed record SavedSearchId(
    string Name,
    string App,
    string Owner);

public sealed record SavedSearch(
    SavedSearchId Id,
    string SplSearchString)
{
    public AlertActionSettings ActionSettings { get; init; } =
        AlertActionSettings.Empty;

    public bool HasEmailAction => ActionSettings.Values.Contains(new KeyValuePair<string, string>("action.email", "1"));
}
