
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SplunkBulkUpdater.Application;       // Adjust to your namespace
using SplunkBulkUpdater.ConsoleApp.Shell;
using SplunkBulkUpdater.ConsoleApp.Shell.Commands;
using SplunkBulkUpdater.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);
var shellAssembly = typeof(IShellCommand).Assembly;

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// register the shell
builder.Services.Scan(scan => scan
    .FromAssemblies(shellAssembly)
    .AddClasses(classes => classes.AssignableTo<IShellCommand>())
    .AsImplementedInterfaces()
    .WithSingletonLifetime());
builder.Services.AddSingleton<SplunkCommandTree>();
builder.Services.AddSingleton<SplunkShell>();

using var host = builder.Build();

await host.StartAsync();
await host.Services.GetRequiredService<SplunkShell>().RunAsync();
await host.StopAsync();
