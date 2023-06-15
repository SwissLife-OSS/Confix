using System.CommandLine;
using Confix.Tool.Commands;
using Confix.Tool.Commands.Component;
using Confix.Tool.Commands.Project;
using Confix.Tool.Commands.Solution;
using Confix.Tool.Commands.Variable;

namespace Confix.Tool;

internal sealed class ConfixRootCommand : Command
{
    public ConfixRootCommand() : base("confix")
    {
        AddGlobalOption(VerbosityOption.Instance);
        AddCommand(new ComponentCommand());
        AddCommand(new VariableCommand());
        AddCommand(new ProjectCommand());
        AddCommand(new ReloadCommand());
        AddCommand(new BuildCommand());
        AddCommand(new SolutionCommand());
    }
}
