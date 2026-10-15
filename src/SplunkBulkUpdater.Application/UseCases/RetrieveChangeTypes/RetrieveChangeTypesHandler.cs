using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.Models.ChangeTypes;
using SplunkBulkUpdater.Application.UseCases.Common;

namespace SplunkBulkUpdater.Application.UseCases.RetrieveChangeTypes
{
    public sealed class RetrieveChangeTypesHandler
        : ICommandHandler<
            RetrieveChangeTypesCommand,
            BaseResult<RetrieveChangeTypesPayload>>
    {
        public Task<BaseResult<RetrieveChangeTypesPayload>> HandleAsync(
            RetrieveChangeTypesCommand command,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var payload = new RetrieveChangeTypesPayload(
            [
                ChangeType.SPL,
                ChangeType.Email,
                ChangeType.Webhook
            ]);

            return Task.FromResult(
                BaseResult<RetrieveChangeTypesPayload>.Success(payload));
        }
    }
}