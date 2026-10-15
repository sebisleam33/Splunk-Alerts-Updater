namespace SplunkBulkUpdater.Application.Models.SavedSearches;

public sealed record SavedSearchId(
    string Name,
    string App,
    string Owner);

public sealed record SavedSearch
{
    public SavedSearchId Id;
    public string SplSearchString;
    public AlertActionSettings ActionSettings { get; init; } =
        AlertActionSettings.Empty;

    public SavedSearch(SavedSearchId id, string splSearchString)
    {
        Id = id;
        SplSearchString = splSearchString;
    }
}
