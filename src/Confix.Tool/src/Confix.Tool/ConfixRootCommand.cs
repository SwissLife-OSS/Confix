using System.CommandLine;
using System.CommandLine.Completions;
using System.CommandLine.Parsing;
using Confix.Tool.Commands.Component;
using Confix.Tool.Commands.Temp;
using Confix.Tool.Commands.Variable;

namespace Confix.Tool;

internal sealed class ConfixRootCommand : Command
{
    public ConfixRootCommand() : base("confix")
    {
        AddGlobalOption(VerbosityOption.Instance);
        AddCommand(new TempCommand());
        AddCommand(new ComponentCommand());
        AddCommand(new VariableCommand());
    }
}