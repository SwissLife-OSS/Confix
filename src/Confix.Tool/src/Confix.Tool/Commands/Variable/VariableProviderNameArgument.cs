using System.CommandLine;

namespace Confix.Tool.Commands.Variable;

internal sealed class VariableProviderNameArgument : Argument<string?>
{
    public static VariableProviderNameArgument Instance { get; } = new();

    public override Type ValueType => base.ValueType;

    private VariableProviderNameArgument()
        : base("provider")
    {
        Arity = ArgumentArity.ZeroOrOne;
        Description = "The name of the provider to resolve the variable from";
    }
}
