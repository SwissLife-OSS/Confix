using System.CommandLine;

namespace Confix.Tool.Commands.Solution;

public sealed class ComponentNameArgument : Argument<string>
{
    private ComponentNameArgument()
        : base("name")
    {
        Arity = ArgumentArity.ExactlyOne;
        Description = "The name of the component";
    }

    public static readonly ComponentNameArgument Instance = new();
}
