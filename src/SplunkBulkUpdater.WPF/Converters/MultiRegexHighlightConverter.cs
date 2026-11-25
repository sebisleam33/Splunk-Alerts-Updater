using SplunkBulkUpdater.WPF.Controls;
using System.Globalization;
using System.Windows.Data;

namespace SplunkBulkUpdater.WPF.Converters;

public sealed class MultiRegexHighlightConverter : IMultiValueConverter
{
    public object Convert(
        object[] values,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        if (values.Length < 2 ||
            values[0] is not string spl ||
            values[1] is not IEnumerable<KeyValuePair<string, string>> filters)
        {
            return Array.Empty<TextHighlightRange>();
        }

        var ranges = new List<TextHighlightRange>();

        foreach (var (field, value) in filters)
        {
            if (string.IsNullOrWhiteSpace(field) ||
                string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            var searchedText = $"{field.Trim()}={value.Trim()}";
            var start = 0;

            while ((start = spl.IndexOf(
                       searchedText,
                       start,
                       StringComparison.OrdinalIgnoreCase)) >= 0)
            {
                ranges.Add(new TextHighlightRange(start, searchedText.Length));
                start += searchedText.Length;
            }
        }

        // Unește porțiunile care se suprapun.
        var merged = new List<TextHighlightRange>();

        foreach (var range in ranges.OrderBy(r => r.Start))
        {
            if (merged.Count == 0)
            {
                merged.Add(range);
                continue;
            }

            var previous = merged[^1];
            var previousEnd = previous.Start + previous.Length;

            if (range.Start <= previousEnd)
            {
                merged[^1] = new TextHighlightRange(
                    previous.Start,
                    Math.Max(previousEnd, range.Start + range.Length) -
                    previous.Start);
            }
            else
            {
                merged.Add(range);
            }
        }

        return merged;
    }

    public object[] ConvertBack(
        object value,
        Type[] targetTypes,
        object parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}