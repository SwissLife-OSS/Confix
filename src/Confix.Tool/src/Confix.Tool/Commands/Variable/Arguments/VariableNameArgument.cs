using System.CommandLine;

namespace Confix.Tool.Commands.Variable;

internal sealed class VariableNameArgument : Argument<string>
{
    public static VariableNameArgument Instance { get; } = new();

    private VariableNameArgument()
        : base("name")
    {
        Arity = ArgumentArity.ExactlyOne;
        Description = "The name of the variable";
    }
}