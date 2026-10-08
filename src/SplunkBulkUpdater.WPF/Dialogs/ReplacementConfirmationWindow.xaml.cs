using SplunkBulkUpdater.Application.UseCases.BulkUpdateSplString;
using System.Windows;

namespace SplunkBulkUpdater.WPF.Dialogs;

public partial class ReplacementConfirmationWindow : Window
{
    public ReplacementConfirmationWindow(
        IReadOnlyList<SplChangePreview> changes)
    {
        InitializeComponent();

        PreviewGrid.ItemsSource = changes;
        SummaryText.Text =
            $"{changes.Count} saved search(es) will be updated.";
    }

    private void ApplyReplacement_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}