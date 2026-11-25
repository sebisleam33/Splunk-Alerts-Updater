using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.Dispatching;
using SplunkBulkUpdater.Application.UseCases.Common;
using System.Reflection;

namespace SplunkBulkUpdater.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddValidatorsFromAssembly(
            assembly,
            ServiceLifetime.Scoped);

        foreach (var type in GetLoadableTypes(assembly)
                     .Where(type => type is { IsClass: true, IsAbstract: false }))
        {
            var handlerInterfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>));

            foreach (var handlerInterface in handlerInterfaces)
            {
                services.AddScoped(handlerInterface, type);
            }
        }

        services.AddScoped<ICommandDispatcher, CommandDispatcher>();

        services.AddSingleton<
            IApplicationExceptionMapper,
            ApplicationExceptionMapper>();

        return services;
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.OfType<Type>();
        }
    }
}