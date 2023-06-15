using System.CommandLine;

namespace Confix.Tool.Commands.Solution;

public sealed class SolutionCommand : Command
{
    public SolutionCommand() : base("solution")
    {
        Description = "This command is used to manage solutions.";

        AddCommand(new SolutionReloadCommand());
        AddCommand(new SolutionBuildCommand());
        AddCommand(new SolutionInitCommand());
    }
}
