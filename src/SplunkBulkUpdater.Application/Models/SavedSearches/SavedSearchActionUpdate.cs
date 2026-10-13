namespace SplunkBulkUpdater.Application.Models.SavedSearches
{
    public sealed record SavedSearchActionUpdate(
        SavedSearchId Id,
        IReadOnlyDictionary<string, string> ChangedValues);
}