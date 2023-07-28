using System.CommandLine;

namespace Confix.Tool.Commands.Variable;

internal sealed class VariableValueOption : Option<string>
{
    public static VariableValueOption Instance { get; } = new();

    private VariableValueOption()
        : base("--value")
    {
        IsRequired = false;
        Description = "The value of the variable";
    }
}
