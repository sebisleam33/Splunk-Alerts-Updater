using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.Abstractions.Persistence;
using SplunkBulkUpdater.Application.Dispatching;
using SplunkBulkUpdater.Application.UseCases.BulkUpdateSplString;
using SplunkBulkUpdater.Application.UseCases.GetAlerts;
using SplunkBulkUpdater.Infrastructure.Persistence;
using SplunkBulkUpdater.Infrastructure.Splunk;
using SplunkBulkUpdater.Infrastructure.Splunk.Configuration;
using SplunkBulkUpdater.WPF.ViewModels;
using System.Windows;

namespace SplunkBulkUpdater.WPF;

public partial class App : System.Windows.Application
{
    private IHost? _host;

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        try
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((_, configuration) =>
                {
                    configuration
                        .SetBasePath(AppContext.BaseDirectory)
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.Configure<SplunkConnectionOptions>(
                        context.Configuration.GetSection("Splunk"));

                    services.AddSingleton<ISplunkServiceFactory, SplunkServiceFactory>();
                    services.AddSingleton<ISavedSearchRepository, SavedSearchRepository>();

                    services.AddSingleton<ICommandDispatcher, CommandDispatcher>();

                    services.AddTransient<IValidator<GetAlertsCommand>, GetAlertsCommandValidator>();
                    services.AddTransient<IValidator<BulkUpdateSplCommand>, BulkUpdateSplCommandValidator>();

                    services.AddTransient<
                        ICommandHandler<GetAlertsCommand,
                            SplunkBulkUpdater.Application.UseCases.Common.BaseResult<
                                IReadOnlyList<GetAlertsPayload>>>,
                        GetAlertsHandler>();

                    services.AddTransient<
                        ICommandHandler<BulkUpdateSplCommand,
                            SplunkBulkUpdater.Application.UseCases.Common.BaseResult<BulkUpdateSplPayload>>,
                        BulkUpdateSplHandler>();

                    services.AddSingleton<MainWindowViewModel>();
                    services.AddSingleton<MainWindow>();
                })
                .Build();

            await _host.StartAsync();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Application startup failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown(-1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        base.OnExit(e);
    }
}