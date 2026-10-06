namespace SplunkBulkUpdater.Application.Models.SavedSearches;

public sealed record SavedSearchQuery(
    string? App = null,
    string? Owner = null,
    bool AlertsOnly = false);