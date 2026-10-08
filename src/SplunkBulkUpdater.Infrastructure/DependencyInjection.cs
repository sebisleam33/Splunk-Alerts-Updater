using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SplunkBulkUpdater.Application.Abstractions.Authentication;
using SplunkBulkUpdater.Application.Abstractions.Persistence;
using SplunkBulkUpdater.Infrastructure.Splunk;
using SplunkBulkUpdater.Infrastructure.Splunk.Authentication;
using SplunkBulkUpdater.Infrastructure.Splunk.Configuration;
using SplunkBulkUpdater.Infrastructure.Splunk.Persistence.RestClientImp;
using SplunkBulkUpdater.Infrastructure.Splunk.Persistence.SplunkSdkImp;

namespace SplunkBulkUpdater.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<
            ISavedSearchRepository,
            SavedSearchSplunkSdkRepository>();

        services.AddScoped<
            ISplunkServiceFactory,
            SplunkServiceFactory>();

        services.AddScoped<
            IAuthenticator,
            Authenticator>();

        services.AddHttpClient<SplunkRestClient>();

        services.AddOptions<SplunkConnectionOptions>()
            .Bind(configuration.GetSection(
                SplunkConnectionOptions.SectionName))
            .ValidateOnStart();

        return services;
    }
}
