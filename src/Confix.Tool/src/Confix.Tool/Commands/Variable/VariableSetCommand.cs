using System.CommandLine;

namespace Confix.Tool.Commands.Variable;

public sealed class VariableSetCommand : Command
{
    public VariableSetCommand() : base("set")
    {
        Description = "sets a variable. Overrides existing value if any.";
    }
}
