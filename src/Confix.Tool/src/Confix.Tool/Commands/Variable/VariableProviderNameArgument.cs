using System.CommandLine;

namespace Confix.Tool.Commands.Variable;

internal sealed class VariableProviderNameOption : Option<string>
{
    public static VariableProviderNameOption Instance { get; } = new();

    private VariableProviderNameOption()
        : base("--provider")
    {
        Arity = ArgumentArity.ExactlyOne;
        Description = "The name of the provider to resolve the variable from";

        AddAlias("-p");
    }
}
