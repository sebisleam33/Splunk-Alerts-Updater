using SplunkBulkUpdater.Application.UseCases.BulkUpdateSplString;
using SplunkBulkUpdater.WPF.Controls;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;


namespace SplunkBulkUpdater.WPF.Dialogs;

public partial class ReplacementConfirmationWindow : Window
{
    private readonly IReadOnlyList<SplChangePreview> _changes;
    private IReadOnlyList<ReplacementPreviewItem> _items = [];

    public ReplacementConfirmationWindow(IReadOnlyList<SplChangePreview> changes)
    {
        ArgumentNullException.ThrowIfNull(changes);
        _changes = changes;
        InitializeComponent();
        _items = changes.Select((change, index) => CreatePreviewItem(change, index)).ToList();
        foreach (var item in _items)
            item.PropertyChanged += PreviewItem_PropertyChanged;

        PreviewGrid.ItemsSource = _items;
        UpdateSelectionSummary();
    }

    public IReadOnlyList<SplChangePreview> SelectedChanges => _items
        .Where(item => item.IsSelected)
        .Select(item => _changes[item.SourceIndex])
        .ToArray();

    private static ReplacementPreviewItem CreatePreviewItem(SplChangePreview change, int sourceIndex)
    {
        var before = change.OriginalSpl ?? string.Empty;
        var after = change.UpdatedSpl ?? string.Empty;
        var (beforeRanges, afterRanges) = CalculateChangedRanges(before, after);
        return new ReplacementPreviewItem(sourceIndex, change.Id.Name, change.Id.App, change.Id.Owner,
            before, after, beforeRanges, afterRanges);
    }

    private static (IReadOnlyList<TextHighlightRange> Before, IReadOnlyList<TextHighlightRange> After)
        CalculateChangedRanges(string before, string after)
    {
        if (string.Equals(before, after, StringComparison.Ordinal))
            return (Array.Empty<TextHighlightRange>(), Array.Empty<TextHighlightRange>());

        var prefix = 0;
        while (prefix < before.Length && prefix < after.Length && before[prefix] == after[prefix]) prefix++;
        var beforeEnd = before.Length;
        var afterEnd = after.Length;
        while (beforeEnd > prefix && afterEnd > prefix && before[beforeEnd - 1] == after[afterEnd - 1])
        { beforeEnd--; afterEnd--; }

        IReadOnlyList<TextHighlightRange> beforeRanges = beforeEnd > prefix
            ? [new TextHighlightRange(prefix, beforeEnd - prefix)] : Array.Empty<TextHighlightRange>();
        IReadOnlyList<TextHighlightRange> afterRanges = afterEnd > prefix
            ? [new TextHighlightRange(prefix, afterEnd - prefix)] : Array.Empty<TextHighlightRange>();
        return (beforeRanges, afterRanges);
    }

    private void PreviewItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ReplacementPreviewItem.IsSelected))
            UpdateSelectionSummary();
    }

    private void UpdateSelectionSummary()
    {
        var selected = _items.Count(item => item.IsSelected);
        SummaryText.Text = $"{selected} of {_items.Count} saved search(es) selected.";
        ApplyButton.IsEnabled = selected > 0;
    }

    private void ApplyReplacement_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedChanges.Count > 0)
            DialogResult = true;
    }

    private void ApplyAll_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox checkBox)
        {
            return;
        }

        var isChecked = checkBox.IsChecked == true;

        foreach (var item in _items)
        {
            item.IsSelected = isChecked;
        }
    }
}

public sealed class ReplacementPreviewItem : INotifyPropertyChanged
{
    private bool _isSelected = true;
    public ReplacementPreviewItem(int sourceIndex, string name, string app, string owner,
        string originalSpl, string updatedSpl,
        IReadOnlyList<TextHighlightRange> beforeRanges,
        IReadOnlyList<TextHighlightRange> afterRanges)
    {
        SourceIndex = sourceIndex; Name = name; App = app; Owner = owner;
        OriginalSpl = originalSpl; UpdatedSpl = updatedSpl;
        BeforeRanges = beforeRanges; AfterRanges = afterRanges;
    }

    public int SourceIndex { get; }
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
    public string Name { get; }
    public string App { get; }
    public string Owner { get; }
    public string OriginalSpl { get; }
    public string UpdatedSpl { get; }
    public IReadOnlyList<TextHighlightRange> BeforeRanges { get; }
    public IReadOnlyList<TextHighlightRange> AfterRanges { get; }
}
