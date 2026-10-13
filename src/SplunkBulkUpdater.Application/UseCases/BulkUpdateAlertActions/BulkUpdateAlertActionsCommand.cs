using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.Models.SavedSearches;
using SplunkBulkUpdater.Application.UseCases.Common;

namespace SplunkBulkUpdater.Application.UseCases.BulkUpdateAlertActions
{
    public sealed record BulkUpdateAlertActionsCommand(
        string App,
        string Owner,
        AlertActionPatch Match,
        AlertActionPatch Replacement,
        bool Preview = true,
        IReadOnlyList<AlertActionChangePreview>? ConfirmedChanges = null)
        : ICommand<BaseResult<BulkUpdateAlertActionsPayload>>;
}