using System.CommandLine;
using Confix.Tool.Commands.Component;
using Confix.Tool.Commands.Temp;

namespace Confix.Tool;

internal sealed class ConfixRootCommand : Command
{
    public ConfixRootCommand() : base("confix")
    {
        AddCommand(new TempCommand());
        AddCommand(new ComponentCommand());
    }
}
