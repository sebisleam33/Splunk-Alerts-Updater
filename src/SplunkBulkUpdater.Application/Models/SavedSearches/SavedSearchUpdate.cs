namespace SplunkBulkUpdater.Application.Models.SavedSearches;

public sealed record SavedSearchUpdate(
    SavedSearchId Id,
    string? SplSearchString = null);
