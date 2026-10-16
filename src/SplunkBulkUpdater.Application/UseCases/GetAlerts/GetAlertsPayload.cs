using SplunkBulkUpdater.Application.Models.ChangeTypes;
using SplunkBulkUpdater.Application.Models.SavedSearches;

namespace SplunkBulkUpdater.Application.UseCases.GetAlerts;

public record GetAlertsPayload(
    SavedSearchId Id,
    string SplSearchString,
    IReadOnlyList<ChangeType> AvailableActions);
