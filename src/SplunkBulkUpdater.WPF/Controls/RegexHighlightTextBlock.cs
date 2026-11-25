using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace SplunkBulkUpdater.WPF.Controls;

public enum RegexHighlightMode
{
    Match,
    Replacement
}

public sealed class RegexHighlightTextBlock : TextBlock
{
    public static readonly DependencyProperty SourceTextProperty =
        DependencyProperty.Register(
            nameof(SourceText),
            typeof(string),
            typeof(RegexHighlightTextBlock),
            new PropertyMetadata(
                string.Empty,
                OnDisplayPropertyChanged));

    public static readonly DependencyProperty PatternProperty =
        DependencyProperty.Register(
            nameof(Pattern),
            typeof(string),
            typeof(RegexHighlightTextBlock),
            new PropertyMetadata(
                string.Empty,
                OnDisplayPropertyChanged));

    public static readonly DependencyProperty ReplacementProperty =
        DependencyProperty.Register(
            nameof(Replacement),
            typeof(string),
            typeof(RegexHighlightTextBlock),
            new PropertyMetadata(
                string.Empty,
                OnDisplayPropertyChanged));

    public static readonly DependencyProperty HighlightModeProperty =
        DependencyProperty.Register(
            nameof(HighlightMode),
            typeof(RegexHighlightMode),
            typeof(RegexHighlightTextBlock),
            new PropertyMetadata(
                RegexHighlightMode.Match,
                OnDisplayPropertyChanged));

    public static readonly DependencyProperty HighlightRangesProperty =
        DependencyProperty.Register(
            nameof(HighlightRanges),
            typeof(IReadOnlyList<TextHighlightRange>),
            typeof(RegexHighlightTextBlock),
            new PropertyMetadata(
                null,
                OnDisplayPropertyChanged));

    public static readonly DependencyProperty HighlightBrushProperty =
        DependencyProperty.Register(
            nameof(HighlightBrush),
            typeof(Brush),
            typeof(RegexHighlightTextBlock),
            new PropertyMetadata(
                Brushes.Gold,
                OnDisplayPropertyChanged));

    public static readonly DependencyProperty HighlightForegroundProperty =
        DependencyProperty.Register(
            nameof(HighlightForeground),
            typeof(Brush),
            typeof(RegexHighlightTextBlock),
            new PropertyMetadata(
                Brushes.Black,
                OnDisplayPropertyChanged));

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

    public string Replacement
    {
        get => (string)GetValue(ReplacementProperty);
        set => SetValue(ReplacementProperty, value);
    }

    public RegexHighlightMode HighlightMode
    {
        get => (RegexHighlightMode)GetValue(HighlightModeProperty);
        set => SetValue(HighlightModeProperty, value);
    }

    public IReadOnlyList<TextHighlightRange>? HighlightRanges
    {
        get => (IReadOnlyList<TextHighlightRange>?)
            GetValue(HighlightRangesProperty);

        set => SetValue(HighlightRangesProperty, value);
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
        if (dependencyObject is RegexHighlightTextBlock control)
            control.RefreshInlines();
    }

    private void RefreshInlines()
    {
        Inlines.Clear();

        var source = SourceText ?? string.Empty;

        if (HighlightRanges is not null)
        {
            RenderRanges(source, HighlightRanges);
            return;
        }

        if (string.IsNullOrWhiteSpace(Pattern))
        {
            Inlines.Add(new Run(source));
            return;
        }

        try
        {
            var regex = new Regex(
                Pattern,
                RegexOptions.None,
                TimeSpan.FromSeconds(1));

            if (HighlightMode == RegexHighlightMode.Match)
                RenderMatches(source, regex);
            else
                RenderReplacement(source, regex);
        }
        catch (ArgumentException)
        {
            Inlines.Clear();
            Inlines.Add(new Run(source));
        }
        catch (RegexMatchTimeoutException)
        {
            Inlines.Clear();
            Inlines.Add(new Run(source));
        }
    }

    private void RenderRanges(
        string source,
        IReadOnlyList<TextHighlightRange> ranges)
    {
        var position = 0;

        foreach (var range in ranges.OrderBy(r => r.Start))
        {
            if (range.Length <= 0 ||
                range.Start < position ||
                range.Start < 0 ||
                range.Start >= source.Length ||
                range.Length > source.Length - range.Start)
            {
                continue;
            }

            if (range.Start > position)
            {
                Inlines.Add(new Run(
                    source[position..range.Start]));
            }

            AddHighlightedRun(
                source.Substring(range.Start, range.Length));

            position = range.Start + range.Length;
        }

        if (position < source.Length)
            Inlines.Add(new Run(source[position..]));
    }

    private void RenderMatches(string source, Regex regex)
    {
        var position = 0;

        foreach (Match match in regex.Matches(source))
        {
            if (!match.Success || match.Length == 0)
                continue;

            if (match.Index > position)
            {
                Inlines.Add(new Run(
                    source[position..match.Index]));
            }

            AddHighlightedRun(match.Value);

            position = match.Index + match.Length;
        }

        if (position < source.Length)
            Inlines.Add(new Run(source[position..]));
    }

    private void RenderReplacement(string source, Regex regex)
    {
        var position = 0;

        foreach (Match match in regex.Matches(source))
        {
            if (!match.Success || match.Length == 0)
                continue;

            if (match.Index > position)
            {
                Inlines.Add(new Run(
                    source[position..match.Index]));
            }

            AddHighlightedRun(match.Result(Replacement ?? string.Empty));

            position = match.Index + match.Length;
        }

        if (position < source.Length)
            Inlines.Add(new Run(source[position..]));
    }

    private void AddHighlightedRun(string text)
    {
        Inlines.Add(new Run(text)
        {
            Background = HighlightBrush,
            Foreground = HighlightForeground,
            FontWeight = FontWeights.Bold
        });
    }
}
