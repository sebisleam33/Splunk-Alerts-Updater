using SplunkBulkUpdater.WPF.Controls;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Media.Media3D;

namespace SplunkBulkUpdater.WPF.Converters
{
    public sealed class MultiRegexHighlightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 ||
                values[0] is not string source ||
                values[1] is not IEnumerable<string> patterns ||
                source.Length == 0)
            {
                return Array.Empty<TextHighlightRange>();
            }

            var ranges = new List<TextHighlightRange>();

            foreach (var pattern in patterns
                .Where(pattern => !string.IsNullOrWhiteSpace(pattern))
                .Distinct(StringComparer.Ordinal))
            {
                try
                {
                    var regex = new Regex(pattern, RegexOptions.None, TimeSpan.FromSeconds(1));
                    var patternRanges = regex.Matches(source)
                        .Cast<Match>()
                        .Where(match => match.Success && match.Length > 0)
                        .Select(match => new TextHighlightRange(match.Index, match.Length))
                        .ToArray();

                    ranges.AddRange(patternRanges);
                }
                catch (ArgumentException)
                {
                    //The user may still be typing the expression.
                }
                catch (RegexMatchTimeoutException)
                {
                    //Skip this expression's highlights.
                }
            }

            if (ranges.Count == 0)
            {
                return Array.Empty<TextHighlightRange>();
            }

            var merged = new List<TextHighlightRange>();

            foreach (var range in ranges
                .OrderBy(range => range.Start)
                .ThenBy(range => range.Length))
            {
                if (merged.Count == 0)
                {
                    merged.Add(range);
                    continue;
                }

                var previous = merged[^1];
                var previousEnd = previous.Start + previous.Length;
                var currentEnd = range.Start + range.Length;

                if (range.Start <= previousEnd)
                {
                    merged[^1] = new TextHighlightRange(previous.Start, Math.Max(previousEnd, currentEnd) - previous.Start);
                }
                else
                {
                    merged.Add(range);
                }
            }

            return merged;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
