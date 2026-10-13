using SplunkBulkUpdater.Application.Models.SavedSearches;
using System.Text.Json;

namespace SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp.v2.RestClient;

internal partial class SplunkRestClient
{
    private static AlertActionSettings ReadActionSettings(
        JsonElement content)
    {
        if (content.ValueKind != JsonValueKind.Object)
        {
            return AlertActionSettings.Empty;
        }

        var values = new Dictionary<string, string>(
            StringComparer.Ordinal);

        foreach (var property in content.EnumerateObject())
        {
            if (!IsActionSetting(property.Name))
            {
                continue;
            }

            values[property.Name] = property.Value.ValueKind switch
            {
                JsonValueKind.String =>
                    property.Value.GetString() ?? string.Empty,

                JsonValueKind.Null =>
                    string.Empty,

                _ =>
                    property.Value.ToString()
            };
        }

        return new AlertActionSettings(values);
    }

    private static bool IsActionSetting(string propertyName)
    {
        return propertyName.Equals(
                   "actions",
                   StringComparison.Ordinal) ||
               propertyName.StartsWith(
                   "action.",
                   StringComparison.Ordinal);
    }
}