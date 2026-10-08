using SplunkBulkUpdater.Application.Models.SavedSearches;

namespace SplunkBulkUpdater.Application.UseCases.BulkUpdateSplString
{
    public sealed record SplChangePreview(
        SavedSearchId Id,
        string OriginalSpl,
        string UpdatedSpl);
}
