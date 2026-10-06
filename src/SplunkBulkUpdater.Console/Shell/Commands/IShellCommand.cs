using System.CommandLine;

namespace SplunkBulkUpdater.ConsoleApp.Shell.Commands;

public interface IShellCommand
{
    string GroupName { get; }
    Command Build();
}