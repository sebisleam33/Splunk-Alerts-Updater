using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.UseCases.Common;

namespace SplunkBulkUpdater.Application.UseCases.RetrieveApps
{
    public sealed record RetrieveAppsCommand
        : ICommand<BaseResult<RetrieveAppsPayload>>;
}
