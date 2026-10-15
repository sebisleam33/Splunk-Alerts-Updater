using SplunkBulkUpdater.Application.UseCases.BulkUpdateAlertActions;
using System.Windows;

namespace SplunkBulkUpdater.WPF.Dialogs;

public partial class AlertActionConfirmationWindow : Window
{
    private readonly IReadOnlyList<AlertActionChangePreview> _changes;
    private readonly IReadOnlyList<ActionPreviewRow> _rows;

    public AlertActionConfirmationWindow(IReadOnlyList<AlertActionChangePreview> changes)
    {
        _changes = changes;
        InitializeComponent();
        _rows = changes.Select((item, index) => new ActionPreviewRow(
            index, item.Id.Name, item.Id.App, item.Id.Owner,
            Format(item.CurrentSettings, item.ChangedSettings.Keys),
            Format(item.ProposedSettings, item.ChangedSettings.Keys))).ToArray();
        ReviewGrid.ItemsSource = _rows;
    }

    public IReadOnlyList<AlertActionChangePreview> SelectedChanges => _rows
        .Where(row => row.IsSelected).Select(row => _changes[row.Index]).ToArray();

    private static string Format(IReadOnlyDictionary<string, string> values, IEnumerable<string> keys) =>
        string.Join(Environment.NewLine, keys.Select(key => $"{key}: {values.GetValueOrDefault(key, "(missing)")}"));

    private void Apply_Click(object sender, RoutedEventArgs e) => DialogResult = true;

    private sealed record ActionPreviewRow(int Index, string Name, string App, string Owner, string Current, string Proposed)
    {
        public bool IsSelected { get; set; } = true;
    }
}
