using System.CommandLine;

namespace Confix.Tool.Commands.Variable;

public sealed class ToEnvironmentOption : Option<string>
{
    public static ToEnvironmentOption Instance { get; } = new();

    private ToEnvironmentOption()
        : base("--to-environment")
    {
        Arity = ArgumentArity.ZeroOrOne;
        Description = "The name of the environment you want to migrate the variable to";
    }
}
