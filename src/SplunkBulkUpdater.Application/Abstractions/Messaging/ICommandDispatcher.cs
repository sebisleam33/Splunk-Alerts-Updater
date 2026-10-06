namespace SplunkBulkUpdater.Application.Abstractions.Messaging;

public interface ICommandDispatcher
{
    Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken ct);
}
