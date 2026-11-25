using SplunkBulkUpdater.Application.Models.SavedSearches;

namespace SplunkBulkUpdater.Application.UseCases.BulkUpdateAlertActions
{
    public sealed record AlertActionChangePreview(
        SavedSearchId Id,
        IReadOnlyDictionary<string, string> CurrentSettings,
        IReadOnlyDictionary<string, string> ProposedSettings,
        IReadOnlyDictionary<string, string> ChangedSettings);
}