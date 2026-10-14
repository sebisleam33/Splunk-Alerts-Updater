using SplunkBulkUpdater.Application.Abstractions.Messaging;

namespace SplunkBulkUpdater.Application.UseCases.RetrieveApps;

public record RetrieveAppsCommand : ICommand<RetrieveAppsPayload>;
