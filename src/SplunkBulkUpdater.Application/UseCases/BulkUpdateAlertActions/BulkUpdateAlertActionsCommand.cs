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
        IReadOnlyDictionary<string, RegexReplacement> Changes,
        bool Preview = true,
        IReadOnlyList<AlertActionChangePreview>? ConfirmedChanges = null,
        IReadOnlySet<SavedSearchId>? SelectedSearches = null)
        : ICommand<BaseResult<BulkUpdateAlertActionsPayload>>;
}
