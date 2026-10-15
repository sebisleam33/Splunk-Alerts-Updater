using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.UseCases.Common;

namespace SplunkBulkUpdater.Application.UseCases.RetrieveChangeTypes
{
    public sealed record RetrieveChangeTypesCommand
        : ICommand<BaseResult<RetrieveChangeTypesPayload>>;
}