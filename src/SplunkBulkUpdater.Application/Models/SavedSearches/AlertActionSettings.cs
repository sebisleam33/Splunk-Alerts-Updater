using SplunkBulkUpdater.Application.Models.ChangeTypes;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace SplunkBulkUpdater.Application.Models.SavedSearches
{
    public sealed class AlertActionSettings
    {
        public static AlertActionSettings Empty { get; } = new([]);

        private static readonly Regex ActionKeyRegex = new(
        @"^action\.[^.]+$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private IReadOnlyList<ChangeType>? _availableActions;

        public AlertActionSettings(
            IEnumerable<KeyValuePair<string, string>> values)
        {
            ArgumentNullException.ThrowIfNull(values);

            Values = new ReadOnlyDictionary<string, string>(
                new Dictionary<string, string>(
                    values,
                    StringComparer.Ordinal));
        }

        public IReadOnlyDictionary<string, string> Values { get; }
        public IReadOnlyList<ChangeType> AvailableActions =>
            _availableActions ??= GetAvailableActions(Values);

        private static IReadOnlyList<ChangeType> GetAvailableActions(
        IReadOnlyDictionary<string, string> values)
        {
            return values
                .Where(pair =>
                    ActionKeyRegex.IsMatch(pair.Key) &&
                    pair.Value == "1")
                .Select(pair => ParseChangeType(pair.Key))
                .ToList();
        }

        public static ChangeType ParseChangeType(string value) =>
        value switch
        {
            "search" or "SPL" => ChangeType.SPL,
            "action.email" => ChangeType.Email,
            "action.webhook" => ChangeType.Webhook,
            _ => throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "Unknown change type.")
        };
    }
}