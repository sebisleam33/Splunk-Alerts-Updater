using SplunkBulkUpdater.Application.UseCases.BulkUpdateSplString;
using SplunkBulkUpdater.WPF.Controls;
using System.Windows;

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
        PreviewGrid.ItemsSource = _items;
        SummaryText.Text = $"{_items.Count} saved search(es) are ready for review.";
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

    private void ApplyReplacement_Click(object sender, RoutedEventArgs e) => DialogResult = true;
}

public sealed class ReplacementPreviewItem
{
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
    public bool IsSelected { get; set; } = true;
    public string Name { get; }
    public string App { get; }
    public string Owner { get; }
    public string OriginalSpl { get; }
    public string UpdatedSpl { get; }
    public IReadOnlyList<TextHighlightRange> BeforeRanges { get; }
    public IReadOnlyList<TextHighlightRange> AfterRanges { get; }
}
