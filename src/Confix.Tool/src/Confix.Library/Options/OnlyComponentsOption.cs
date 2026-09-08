using System.CommandLine;

namespace Confix.Tool;

public sealed class OnlyComponentsOption : Option<bool>
{
    public OnlyComponentsOption() : base("--only-components")
    {
        Description = "If you specify this option, only the components will be built.";
        Required = false;
        Arity = ArgumentArity.ZeroOrOne;
    }

    public static OnlyComponentsOption Instance { get; } = new();
}
