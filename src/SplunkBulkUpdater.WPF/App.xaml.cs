using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SplunkBulkUpdater.Application;
using SplunkBulkUpdater.Infrastructure;
using SplunkBulkUpdater.WPF.Dialogs;
using SplunkBulkUpdater.WPF.ViewModels;
using System.Windows;
using SplunkBulkUpdater.Application.Abstractions.Authentication;

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
                    services.AddScoped<
                        IAlertActionConfirmationService,
                        AlertActionConfirmationService>();

                    services.AddSingleton<SignedInUserContext>();

                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<LoginWindow>();
                    services.AddTransient<LogoutConfirmationWindow>();

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

            ShowMainWindow(scopedServices);
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

    private void ShowMainWindow(IServiceProvider services)
    {
        var mainWindow = services.GetRequiredService<MainWindow>();

        mainWindow.LogoutRequested += MainWindow_LogoutRequested;

        MainWindow = mainWindow;
        ShutdownMode = ShutdownMode.OnMainWindowClose;

        mainWindow.Show();
    }

    private void MainWindow_LogoutRequested(object? sender, EventArgs e)
    {
        if (sender is not MainWindow mainWindow ||
            _applicationScope is null)
        {
            return;
        }

        var services = _applicationScope.ServiceProvider;

        var confirmationWindow =
            services.GetRequiredService<LogoutConfirmationWindow>();

        confirmationWindow.Owner = mainWindow;

        var confirmed = confirmationWindow.ShowDialog() == true;

        if (!confirmed)
        {
            return;
        }

        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var authenticator =
            services.GetRequiredService<IAuthenticator>();

        var userContext =
            services.GetRequiredService<SignedInUserContext>();

        authenticator.SignOut();
        userContext.Clear();

        mainWindow.LogoutRequested -= MainWindow_LogoutRequested;
        mainWindow.Close();

        var loginWindow =
            services.GetRequiredService<LoginWindow>();

        var loginSucceeded = loginWindow.ShowDialog() == true;

        if (!loginSucceeded)
        {
            Shutdown();
            return;
        }

        ShowMainWindow(services);
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
