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
                .Select(pair => TryParseChangeType(pair.Key, out var changeType)
                    ? (ChangeType?)changeType
                    : null)
                .Where(changeType => changeType.HasValue)
                .Select(changeType => changeType!.Value)
                .ToList();
        }

        private static bool TryParseChangeType(
            string value,
            out ChangeType changeType)
        {
            switch (value)
            {
                case "search":
                case "SPL":
                    changeType = ChangeType.SPL;
                    return true;

                case "action.email":
                    changeType = ChangeType.Email;
                    return true;

                case "action.webhook":
                    changeType = ChangeType.Webhook;
                    return true;

                default:
                    changeType = default;
                    return false;
            }
        }
    }
}