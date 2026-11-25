namespace SplunkBulkUpdater.Application.Models.SavedSearches;

public sealed record UpdateResult(
    SavedSearchId Id,
    bool Succeeded,
    string? Error = null);
