namespace SplunkBulkUpdater.Application.Models.SavedSearches;

public sealed record BulkUpdateResult(
    IReadOnlyList<UpdateResult> Items)
{
    public int SucceededCount => Items.Count(item => item.Succeeded);
    public int FailedCount => Items.Count(item => !item.Succeeded);
}
