using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace SplunkBulkUpdater.WPF.Controls;

public sealed class RegexHighlightTextBlock : TextBlock
{
    public static readonly DependencyProperty SourceTextProperty =
        DependencyProperty.Register(
            nameof(SourceText),
            typeof(string),
            typeof(RegexHighlightTextBlock),
            new PropertyMetadata(string.Empty, OnDisplayPropertyChanged));

    public static readonly DependencyProperty PatternProperty =
        DependencyProperty.Register(
            nameof(Pattern),
            typeof(string),
            typeof(RegexHighlightTextBlock),
            new PropertyMetadata(string.Empty, OnDisplayPropertyChanged));

    public static readonly DependencyProperty HighlightBrushProperty =
        DependencyProperty.Register(
            nameof(HighlightBrush),
            typeof(Brush),
            typeof(RegexHighlightTextBlock),
            new PropertyMetadata(Brushes.Gold, OnDisplayPropertyChanged));

    public static readonly DependencyProperty HighlightForegroundProperty =
        DependencyProperty.Register(
            nameof(HighlightForeground),
            typeof(Brush),
            typeof(RegexHighlightTextBlock),
            new PropertyMetadata(Brushes.Black, OnDisplayPropertyChanged));

    public string SourceText
    {
        get => (string)GetValue(SourceTextProperty);
        set => SetValue(SourceTextProperty, value);
    }

    public string Pattern
    {
        get => (string)GetValue(PatternProperty);
        set => SetValue(PatternProperty, value);
    }

    public Brush HighlightBrush
    {
        get => (Brush)GetValue(HighlightBrushProperty);
        set => SetValue(HighlightBrushProperty, value);
    }

    public Brush HighlightForeground
    {
        get => (Brush)GetValue(HighlightForegroundProperty);
        set => SetValue(HighlightForegroundProperty, value);
    }

    private static void OnDisplayPropertyChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs e)
    {
        ((RegexHighlightTextBlock)dependencyObject).RefreshInlines();
    }

    private void RefreshInlines()
    {
        Inlines.Clear();

        var text = SourceText ?? string.Empty;
        var pattern = Pattern ?? string.Empty;

        if (string.IsNullOrEmpty(text))
            return;

        if (string.IsNullOrWhiteSpace(pattern))
        {
            Inlines.Add(new Run(text));
            return;
        }

        try
        {
            var regex = new Regex(
                pattern,
                RegexOptions.None,
                TimeSpan.FromSeconds(2));

            var cursor = 0;

            foreach (Match match in regex.Matches(text))
            {
                // A zero-length match has no visible text to highlight.
                if (!match.Success || match.Length == 0)
                    continue;

                if (match.Index > cursor)
                {
                    Inlines.Add(new Run(text[cursor..match.Index]));
                }

                Inlines.Add(new Run(match.Value)
                {
                    Background = HighlightBrush,
                    Foreground = HighlightForeground,
                    FontWeight = FontWeights.Bold
                });

                cursor = match.Index + match.Length;
            }

            if (cursor < text.Length)
            {
                Inlines.Add(new Run(text[cursor..]));
            }
        }
        catch (ArgumentException)
        {
            // Keep the SPL visible if the pattern is invalid.
            Inlines.Add(new Run(text));
        }
        catch (RegexMatchTimeoutException)
        {
            // Keep the SPL visible if evaluating the pattern times out.
            Inlines.Add(new Run(text));
        }
    }
}