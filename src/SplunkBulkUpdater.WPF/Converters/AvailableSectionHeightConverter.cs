using System.Globalization;
using System.Windows.Data;

namespace SplunkBulkUpdater.WPF.Converters;

/// <summary>
/// Limits Change fields while reserving space for the header,
/// alert cards and footer.
/// </summary>
public sealed class AvailableSectionHeightConverter : IMultiValueConverter
{
    public object Convert(
        object[] values,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        if (values.Length != 3 ||
            values.Any(value => value is not double))
        {
            return 220d;
        }

        var viewport = (double)values[0];
        var header = (double)values[1];
        var footer = (double)values[2];

        if (viewport <= 0)
        {
            return 220d;
        }

        var minimumAlertsHeight = double.TryParse(
            parameter?.ToString(),
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var minimum)
                ? minimum
                : 270d;

        // Header bottom: 16; footer top: 4; alert cards bottom: 4.
        const double spacing = 24d;

        return Math.Clamp(
            viewport - header - footer - minimumAlertsHeight - spacing,
            140d,
            440d);
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