using SplunkBulkUpdater.Application.UseCases.Common;

namespace SplunkBulkUpdater.Application.Abstractions.Messaging;

public interface ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
    where TResult : BaseResult
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken);
}
