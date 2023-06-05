using System.CommandLine;

namespace Confix.Tool.Commands.Variable;

internal sealed class VariableNameArgument : Argument<string>
{
    public static VariableNameArgument Instance { get; } = new();

    public override Type ValueType => base.ValueType;

    private VariableNameArgument()
        : base("variable-name")
    {
        Arity = ArgumentArity.ExactlyOne;
        Description = "The name of the variable to resolve";
    }
}
