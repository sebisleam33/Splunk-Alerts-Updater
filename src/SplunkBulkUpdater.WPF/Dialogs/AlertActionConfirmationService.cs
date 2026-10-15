using SplunkBulkUpdater.Application.UseCases.BulkUpdateAlertActions;
using System.Windows;

namespace SplunkBulkUpdater.WPF.Dialogs;

public sealed class AlertActionConfirmationService : IAlertActionConfirmationService
{
    public IReadOnlyList<AlertActionChangePreview>? Confirm(IReadOnlyList<AlertActionChangePreview> changes)
    {
        var dialog = new AlertActionConfirmationWindow(changes) { Owner = System.Windows.Application.Current.MainWindow };
        return dialog.ShowDialog() == true ? dialog.SelectedChanges : null;
    }
}
