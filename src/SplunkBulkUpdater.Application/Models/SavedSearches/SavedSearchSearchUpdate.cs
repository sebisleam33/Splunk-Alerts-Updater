namespace SplunkBulkUpdater.Application.Models.SavedSearches;

public sealed record SavedSearchSearchUpdate(
    SavedSearchId Id,
    string SplSearchString);
