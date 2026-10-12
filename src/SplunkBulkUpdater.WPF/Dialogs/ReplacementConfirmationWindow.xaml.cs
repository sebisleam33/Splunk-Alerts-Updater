using SplunkBulkUpdater.Application.UseCases.BulkUpdateSplString;
using SplunkBulkUpdater.WPF.Controls;
using System.Windows;

namespace SplunkBulkUpdater.WPF.Dialogs;

public partial class ReplacementConfirmationWindow : Window
{
    public ReplacementConfirmationWindow(
        IReadOnlyList<SplChangePreview> changes)
    {
        ArgumentNullException.ThrowIfNull(changes);

        InitializeComponent();

        var items = changes
            .Select(CreatePreviewItem)
            .ToList();

        PreviewGrid.ItemsSource = items;

        SummaryText.Text =
            $"{items.Count} saved search(es) will be updated.";
    }

    private static ReplacementPreviewItem CreatePreviewItem(
        SplChangePreview change)
    {
        var before = change.OriginalSpl ?? string.Empty;
        var after = change.UpdatedSpl ?? string.Empty;

        var (beforeRanges, afterRanges) =
            CalculateChangedRanges(before, after);

        return new ReplacementPreviewItem(
            change.Id.Name,
            change.Id.App,
            change.Id.Owner,
            before,
            after,
            beforeRanges,
            afterRanges);
    }

    private static (
        IReadOnlyList<TextHighlightRange> Before,
        IReadOnlyList<TextHighlightRange> After)
        CalculateChangedRanges(string before, string after)
    {
        if (string.Equals(
                before,
                after,
                StringComparison.Ordinal))
        {
            return (
                Array.Empty<TextHighlightRange>(),
                Array.Empty<TextHighlightRange>());
        }

        var prefix = 0;

        while (prefix < before.Length &&
               prefix < after.Length &&
               before[prefix] == after[prefix])
        {
            prefix++;
        }

        var beforeEnd = before.Length;
        var afterEnd = after.Length;

        while (beforeEnd > prefix &&
               afterEnd > prefix &&
               before[beforeEnd - 1] == after[afterEnd - 1])
        {
            beforeEnd--;
            afterEnd--;
        }

        IReadOnlyList<TextHighlightRange> beforeRanges =
            beforeEnd > prefix
                ? new[]
                {
                    new TextHighlightRange(
                        prefix,
                        beforeEnd - prefix)
                }
                : Array.Empty<TextHighlightRange>();

        IReadOnlyList<TextHighlightRange> afterRanges =
            afterEnd > prefix
                ? new[]
                {
                    new TextHighlightRange(
                        prefix,
                        afterEnd - prefix)
                }
                : Array.Empty<TextHighlightRange>();

        return (beforeRanges, afterRanges);
    }

    private void ApplyReplacement_Click(
        object sender,
        RoutedEventArgs e)
    {
        DialogResult = true;
    }
}

public sealed record ReplacementPreviewItem(
    string Name,
    string App,
    string Owner,
    string OriginalSpl,
    string UpdatedSpl,
    IReadOnlyList<TextHighlightRange> BeforeRanges,
    IReadOnlyList<TextHighlightRange> AfterRanges);
