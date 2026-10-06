using Microsoft.Extensions.DependencyInjection;
using SplunkBulkUpdater.Application.Abstractions.Messaging;

namespace SplunkBulkUpdater.Application.Dispatching;

public class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public CommandDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken ct)
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));
        dynamic handler = _serviceProvider.GetRequiredService(handlerType);
        return handler.HandleAsync((dynamic)command, ct);
    }
}
