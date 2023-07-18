using System.CommandLine;

namespace Confix.Tool.Commands.Variable;

internal sealed class FromVariableNameArgument : Argument<string>
{
    public static FromVariableNameArgument Instance { get; } = new();

    private FromVariableNameArgument()
        : base("from")
    {
        Arity = ArgumentArity.ExactlyOne;
        Description = "The name of the new variable";
    }
}
