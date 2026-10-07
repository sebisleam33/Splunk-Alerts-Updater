using Microsoft.Extensions.DependencyInjection;
using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.UseCases.GetAlerts;
using System.Text.Json;

namespace SplunkBulkUpdater.ConsoleApp.Shell.Commands;

public sealed class GetAlertsCliCommand(IServiceScopeFactory scopeFactory) : IShellCommand
{
    public string GroupName => "alerts";

    public System.CommandLine.Command Build()
    {
        var splRegexOption = new System.CommandLine.Option<string>("--spl-regex")
        {
            Required = true,
            Description = "Regex used to select saved searches"
        };

        var appOption = new System.CommandLine.Option<string?>("--app")
        {
            Description = "Splunk app context"
        };

        var ownerOption = new System.CommandLine.Option<string?>("--owner")
        {
            Description = "Splunk owner context"
        };

        var command = new System.CommandLine.Command(
            "get",
            "Retrieve matching saved-search alerts.")
        {
            splRegexOption,
            appOption,
            ownerOption
        };

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var username = Environment.GetEnvironmentVariable("SPLUNK_USERNAME");

            if (string.IsNullOrWhiteSpace(username))
            {
                Console.Error.WriteLine(
                    "Set the SPLUNK_PASSWORD environment variable before starting the shell.");
                return 1;
            }

            var password = Environment.GetEnvironmentVariable("SPLUNK_PASSWORD");

            if (string.IsNullOrWhiteSpace(password))
            {
                Console.Error.WriteLine(
                    "Set the SPLUNK_PASSWORD environment variable before starting the shell.");
                return 1;
            }

            var applicationCommand = new GetAlertsCommand(
                username: username,
                password: password,
                SplRegex: parseResult.GetValue(splRegexOption)!,
                App: parseResult.GetValue(appOption),
                Owner: parseResult.GetValue(ownerOption));

            await using var scope = scopeFactory.CreateAsyncScope();
            var dispatcher =
                scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();

            var result = await dispatcher.Send(applicationCommand, cancellationToken);

            Console.WriteLine(JsonSerializer.Serialize(
                result,
                new JsonSerializerOptions { WriteIndented = true }));

            return 0;
        });

        return command;
    }
}