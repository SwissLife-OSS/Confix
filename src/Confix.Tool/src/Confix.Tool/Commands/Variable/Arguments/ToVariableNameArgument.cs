using System.CommandLine;

namespace Confix.Tool.Commands.Variable;

internal sealed class ToVariableNameArgument : Argument<string>
{
    public static ToVariableNameArgument Instance { get; } = new();

    private ToVariableNameArgument()
        : base("to")
    {
        Arity = ArgumentArity.ExactlyOne;
        Description = "The name of the new variable";
    }
}
