using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.Models.ChangeTypes;
using SplunkBulkUpdater.Application.Models.SavedSearches;
using SplunkBulkUpdater.Application.UseCases.Common;

namespace SplunkBulkUpdater.Application.UseCases.BulkUpdateAlertActions
{
    public sealed record BulkUpdateAlertActionsCommand(
        string App,
        string Owner,
        ChangeType ChangeType,
        IReadOnlyDictionary<string, ReplacementPair> Changes,
        bool Preview = true,
        IReadOnlyList<AlertActionChangePreview>? ConfirmedChanges = default,
        IReadOnlySet<SavedSearchId>? SelectedSearches = default,
        IReadOnlyDictionary<string, string>? SearchStringFilters = null)
        : ICommand<BaseResult<BulkUpdateAlertActionsPayload>>;
}
