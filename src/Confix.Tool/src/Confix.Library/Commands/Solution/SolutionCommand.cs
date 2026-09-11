using System.CommandLine;

namespace Confix.Tool.Commands.Solution;

public sealed class SolutionCommand : Command
{
    public SolutionCommand() : base("solution")
    {
        Description = "This command is used to manage solutions.";

        Add(new SolutionRestoreCommand());
        Add(new SolutionBuildCommand());
        Add(new SolutionInitCommand());
        Add(new SolutionValidateCommand());
    }
}
