using SplunkBulkUpdater.ConsoleApp.Shell.Commands;
using System.CommandLine;

namespace SplunkBulkUpdater.ConsoleApp.Shell;

public sealed class SplunkCommandTree(IEnumerable<IShellCommand> commands)
{
    public RootCommand Build()
    {
        var root = new RootCommand("Splunk Bulk Updater");

        foreach (var group in commands.GroupBy(command => command.GroupName))
        {
            var groupCommand = new Command(group.Key);

            foreach (var shellCommand in group)
            {
                groupCommand.Subcommands.Add(shellCommand.Build());
            }

            root.Subcommands.Add(groupCommand);
        }

        return root;
    }
}