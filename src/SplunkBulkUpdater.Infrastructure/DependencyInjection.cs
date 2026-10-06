using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SplunkBulkUpdater.Application.Abstractions.Persistence;
using SplunkBulkUpdater.Infrastructure.Persistence;
using SplunkBulkUpdater.Infrastructure.Splunk;
using SplunkBulkUpdater.Infrastructure.Splunk.Configuration;

namespace SplunkBulkUpdater.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<ISavedSearchRepository, SavedSearchRepository>();
        services.AddScoped<ISplunkServiceFactory, SplunkServiceFactory>();

        services.AddOptions<SplunkConnectionOptions>()
            .Bind(configuration.GetSection(SplunkConnectionOptions.SectionName))
            .ValidateOnStart();

        return services;
    }
}
