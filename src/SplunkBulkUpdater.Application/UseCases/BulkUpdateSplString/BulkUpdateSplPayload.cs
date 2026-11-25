namespace SplunkBulkUpdater.Application.UseCases.BulkUpdateSplString
{
    public record BulkUpdateSplPayload(
        int SucceededCount,
        int FailedCount,
        IReadOnlyList<SplChangePreview> Changes);
}
