using SplunkBulkUpdater.Application.Models.SavedSearches;

namespace SplunkBulkUpdater.Application.UseCases.BulkUpdateAlertActions
{
    public sealed record BulkUpdateAlertActionsPayload(
        int SucceededCount,
        int FailedCount,
        IReadOnlyList<AlertActionChangePreview> Changes,
        IReadOnlyList<UpdateResult> Results);
}