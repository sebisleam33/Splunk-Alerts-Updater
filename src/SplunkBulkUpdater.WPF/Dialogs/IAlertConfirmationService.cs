using SplunkBulkUpdater.Application.UseCases.BulkUpdateAlertActions;

namespace SplunkBulkUpdater.WPF.Dialogs;

public interface IAlertActionConfirmationService
{
    IReadOnlyList<AlertActionChangePreview>? Confirm(IReadOnlyList<AlertActionChangePreview> changes);
}
