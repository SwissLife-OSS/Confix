using System.CommandLine;

namespace Confix.Tool.Commands.Project;

public sealed class ListCommand : Command
{
    public ListCommand() : base("list")
    {
        Description = "This command is used to list different informations.";

        AddCommand(new ListProjectsCommand());
    }
}
