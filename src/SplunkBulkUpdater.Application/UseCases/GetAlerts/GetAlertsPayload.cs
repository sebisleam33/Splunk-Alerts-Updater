using SplunkBulkUpdater.Application.Models.SavedSearches;

namespace SplunkBulkUpdater.Application.UseCases.GetAlerts;

public record GetAlertsPayload(
    SavedSearch SavedSearch);
