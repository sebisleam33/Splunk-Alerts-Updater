using SplunkBulkUpdater.Application.Models.SavedSearches;

namespace SplunkBulkUpdater.Application.UseCases.GetAlerts;

public record GetAlertsPayload(
    SavedSearchId Id,
    string SplSearchString);
