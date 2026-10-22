using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace SplunkBulkUpdater.WPF.Controls;

public sealed class SelectableHighlightedText : RichTextBox
{
    public static readonly DependencyProperty SourceTextProperty =
        DependencyProperty.Register(
            nameof(SourceText),
            typeof(string),
            typeof(SelectableHighlightedText),
            new PropertyMetadata(string.Empty, OnContentChanged));

    public static readonly DependencyProperty HighlightRangesProperty =
        DependencyProperty.Register(
            nameof(HighlightRanges),
            typeof(IReadOnlyList<TextHighlightRange>),
            typeof(SelectableHighlightedText),
            new PropertyMetadata(null, OnContentChanged));

    public string SourceText
    {
        get => (string)GetValue(SourceTextProperty);
        set => SetValue(SourceTextProperty, value);
    }

    public IReadOnlyList<TextHighlightRange>? HighlightRanges
    {
        get => (IReadOnlyList<TextHighlightRange>?)
            GetValue(HighlightRangesProperty);
        set => SetValue(HighlightRangesProperty, value);
    }

    public SelectableHighlightedText()
    {
        IsReadOnly = true;
        IsUndoEnabled = false;
        BorderThickness = new Thickness(0);
        Padding = new Thickness(0);
        Background = Brushes.Transparent;
        VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
        HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
    }

    private static void OnContentChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is SelectableHighlightedText control)
        {
            control.RefreshDocument();
        }
    }

    private void RefreshDocument()
    {
        var text = SourceText ?? string.Empty;
        var paragraph = new Paragraph { Margin = new Thickness(0) };
        var position = 0;

        foreach (var range in (HighlightRanges ?? [])
                     .OrderBy(range => range.Start))
        {
            if (range.Length <= 0 ||
                range.Start < position ||
                range.Start < 0 ||
                range.Start + range.Length > text.Length)
            {
                continue;
            }

            if (range.Start > position)
            {
                paragraph.Inlines.Add(
                    new Run(text[position..range.Start]));
            }

            paragraph.Inlines.Add(new Run(
                text.Substring(range.Start, range.Length))
            {
                Background = Brushes.Gold,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold
            });

            position = range.Start + range.Length;
        }

        if (position < text.Length)
        {
            paragraph.Inlines.Add(new Run(text[position..]));
        }

        var document = new FlowDocument(paragraph)
        {
            PagePadding = new Thickness(0)
        };

        BindingOperations.SetBinding(
            document,
            TextElement.ForegroundProperty,
            new Binding(nameof(Foreground)) { Source = this });

        Document = document;
    }
}