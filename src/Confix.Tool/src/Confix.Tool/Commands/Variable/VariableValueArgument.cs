using System.CommandLine;

namespace Confix.Tool.Commands.Variable;

internal sealed class VariableValueArgument : Argument<string>
{
    public static VariableValueArgument Instance { get; } = new();

    public override Type ValueType => base.ValueType;

    private VariableValueArgument()
        : base("value")
    {
        Arity = ArgumentArity.ExactlyOne;
        Description = "The value of the variable";
    }
}
