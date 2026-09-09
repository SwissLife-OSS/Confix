using System.CommandLine;

namespace Confix.Tool.Commands.Variable;

public sealed class VariableCommand : Command
{
    public VariableCommand() : base("variable")
    {
        Description = "This command is used to manage variables.";
        Add(new VariableGetCommand());
        Add(new VariableSetCommand());
        Add(new VariableListCommand());
        Add(new VariableCopyCommand());
    }
}
