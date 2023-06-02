using System.CommandLine;
using Confix.Tool.Commands.Component;
using Confix.Tool.Commands.Temp;
using Confix.Tool.Commands.Variable;

namespace Confix.Tool;

internal sealed class ConfixRootCommand : Command
{
    public ConfixRootCommand() : base("confix")
    {
        AddCommand(new TempCommand());
        AddCommand(new ComponentCommand());
        AddCommand(new VariableCommand());
    }
}
