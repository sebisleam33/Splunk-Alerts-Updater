using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.UseCases.Common;

namespace SplunkBulkUpdater.Application.UseCases.RetrieveOwnershipFilter
{
    public sealed class RetrieveOwnershipFilterHandler
        : ICommandHandler<
            RetrieveOwnershipFilterCommand,
            BaseResult<RetrieveOwnershipFilterPayload>>
    {
        public Task<BaseResult<RetrieveOwnershipFilterPayload>> HandleAsync(
            RetrieveOwnershipFilterCommand command,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var payload = new RetrieveOwnershipFilterPayload(
                [OwnershipFilter.Yours]);

            return Task.FromResult(
                BaseResult<RetrieveOwnershipFilterPayload>.Success(payload));
        }
    }
}