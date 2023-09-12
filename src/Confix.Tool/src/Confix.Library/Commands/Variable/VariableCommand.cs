using System.CommandLine;

namespace Confix.Tool.Commands.Variable;

public sealed class VariableCommand : Command
{
    public VariableCommand() : base("variable")
    {
        Description = "This command is used to manage variables.";
        AddCommand(new VariableGetCommand());
        AddCommand(new VariableSetCommand());
        AddCommand(new VariableListCommand());
        AddCommand(new VariableCopyCommand());
    }
}
