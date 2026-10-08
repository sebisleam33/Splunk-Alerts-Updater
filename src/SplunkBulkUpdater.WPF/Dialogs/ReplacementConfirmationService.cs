using SplunkBulkUpdater.Application.UseCases.BulkUpdateSplString;
using System.Windows;

namespace SplunkBulkUpdater.WPF.Dialogs;

public sealed class ReplacementConfirmationService
    : IReplacementConfirmationService
{
    public bool Confirm(IReadOnlyList<SplChangePreview> changes)
    {
        var dialog = new ReplacementConfirmationWindow(changes)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        return dialog.ShowDialog() == true;
    }
}