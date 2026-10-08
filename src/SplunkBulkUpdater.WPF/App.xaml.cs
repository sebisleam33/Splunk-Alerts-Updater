using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SplunkBulkUpdater.Application;
using SplunkBulkUpdater.Infrastructure;
using SplunkBulkUpdater.WPF.Dialogs;
using SplunkBulkUpdater.WPF.ViewModels;
using System.Windows;

namespace SplunkBulkUpdater.WPF;

public partial class App : System.Windows.Application
{
    private IHost? _host;
    private IServiceScope? _applicationScope;

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        try
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((_, configuration) =>
                {
                    configuration
                        .SetBasePath(AppContext.BaseDirectory)
                        .AddJsonFile(
                            "appsettings.json",
                            optional: false,
                            reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddApplication();
                    services.AddInfrastructure(context.Configuration);

                    services.AddScoped<
                        IReplacementConfirmationService,
                        ReplacementConfirmationService>();

                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<LoginWindow>();

                    services.AddTransient<MainWindowViewModel>();
                    services.AddTransient<MainWindow>();
                })
                .Build();

            await _host.StartAsync();

            _applicationScope = _host.Services.CreateScope();
            var scopedServices = _applicationScope.ServiceProvider;

            var loginWindow = scopedServices.GetRequiredService<LoginWindow>();
            var loginSucceeded = loginWindow.ShowDialog() == true;

            if (!loginSucceeded)
            {
                Shutdown();
                return;
            }

            var mainWindow = scopedServices.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            ShutdownMode = ShutdownMode.OnMainWindowClose;
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
        _applicationScope?.Dispose();

        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        base.OnExit(e);
    }
}