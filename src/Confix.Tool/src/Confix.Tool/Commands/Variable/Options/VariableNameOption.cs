using System.CommandLine;

namespace Confix.Tool.Commands.Variable;

public sealed class VariableNameOption : Option<string>
{
    public static VariableNameOption Instance { get; } = new();

    private VariableNameOption()
        : base("--name")
    {
        IsRequired = false;
        Description = "The name of the variable";
    }
}