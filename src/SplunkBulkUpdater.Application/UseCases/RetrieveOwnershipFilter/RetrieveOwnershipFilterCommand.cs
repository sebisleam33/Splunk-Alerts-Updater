using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.UseCases.Common;

namespace SplunkBulkUpdater.Application.UseCases.RetrieveOwnershipFilter
{
    public sealed record RetrieveOwnershipFilterCommand
        : ICommand<BaseResult<RetrieveOwnershipFilterPayload>>;
}