using System.CommandLine;

namespace Confix.Tool.Commands.Variable;

public sealed class ToVariableNameOption : Option<string>
{
    public static ToVariableNameOption Instance { get; } = new();

    private ToVariableNameOption()
        : base("--to")
    {
        IsRequired = false;
        Description = "The name of the new variable";
    }
}
