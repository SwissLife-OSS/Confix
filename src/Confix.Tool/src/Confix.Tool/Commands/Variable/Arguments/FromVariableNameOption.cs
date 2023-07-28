using System.CommandLine;

namespace Confix.Tool.Commands.Variable;

public sealed class FromVariableNameOption : Option<string>
{
    public static FromVariableNameOption Instance { get; } = new();

    private FromVariableNameOption()
        : base("--from")
    {
        IsRequired = false;
        Description = "The name of the new variable";
    }
}
