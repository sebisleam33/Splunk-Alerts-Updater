namespace SplunkBulkUpdater.Application;

using FluentValidation;

// Application/DependencyInjection.cs
using Microsoft.Extensions.DependencyInjection;
using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.Dispatching;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(
            typeof(DependencyInjection).Assembly,
            ServiceLifetime.Scoped);

        services.Scan(scan => scan
            .FromAssemblies(typeof(DependencyInjection).Assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        services.AddScoped<ICommandDispatcher, CommandDispatcher>();

        return services;
    }
}
