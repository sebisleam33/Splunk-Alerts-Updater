using SplunkBulkUpdater.Application.UseCases.BulkUpdateAlertActions;
using System.ComponentModel;
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
            foreach (var row in _rows)
                row.PropertyChanged += PreviewRow_PropertyChanged;
        ReviewGrid.ItemsSource = _rows;
        UpdateSelectionSummary();
    }

    public IReadOnlyList<AlertActionChangePreview> SelectedChanges => _rows
        .Where(row => row.IsSelected).Select(row => _changes[row.Index]).ToArray();

    private static string Format(IReadOnlyDictionary<string, string> values, IEnumerable<string> keys) =>
        string.Join(Environment.NewLine, keys.Select(key => $"{key}: {values.GetValueOrDefault(key, "(missing)")}"));

    private void PreviewRow_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ActionPreviewRow.IsSelected))
            UpdateSelectionSummary();
    }

    private void UpdateSelectionSummary()
    {
        var selected = _rows.Count(row => row.IsSelected);
        SummaryText.Text = $"{selected} of {_rows.Count} alert(s) selected.";
        ApplyButton.IsEnabled = selected > 0;
    }

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedChanges.Count > 0)
            DialogResult = true;
    }
}

public sealed record ActionPreviewRow(
    int Index,
    string Name,
    string App,
    string Owner,
    string Current,
    string Proposed) : INotifyPropertyChanged
{
    private bool _isSelected = true;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;

            _isSelected = value;
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;
}
