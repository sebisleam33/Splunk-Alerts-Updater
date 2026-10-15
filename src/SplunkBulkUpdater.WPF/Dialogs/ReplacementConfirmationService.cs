using SplunkBulkUpdater.Application.UseCases.BulkUpdateSplString;

namespace SplunkBulkUpdater.WPF.Dialogs;

public sealed class ReplacementConfirmationService
    : IReplacementConfirmationService
{
    public IReadOnlyList<SplChangePreview>? Confirm(IReadOnlyList<SplChangePreview> changes)
    {
        var dialog = new ReplacementConfirmationWindow(changes)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        return dialog.ShowDialog() == true ? dialog.SelectedChanges : null;
    }
}