namespace SplunkBulkUpdater.ConsoleApp.Shell;

public sealed class SplunkShell(SplunkCommandTree commandTree)
{
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var root = commandTree.Build();

        Console.WriteLine("Splunk Bulk Updater. Type 'help' for commands or 'exit' to quit.");

        while (!cancellationToken.IsCancellationRequested)
        {
            Console.Write("> ");
            var line = Console.ReadLine();

            if (line is null ||
                line.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                line.Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.Equals("help", StringComparison.OrdinalIgnoreCase))
                line = "--help";

            try
            {
                var parseResult = root.Parse(line);
                await parseResult.InvokeAsync(cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Command failed: {ex.Message}");
            }
        }
    }
}