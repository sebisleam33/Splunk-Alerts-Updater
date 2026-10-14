namespace SplunkBulkUpdater.Application.UseCases.RetrieveApps;

public record RetrieveAppsPayload(
    IReadOnlyDictionary<string, string> Apps);
