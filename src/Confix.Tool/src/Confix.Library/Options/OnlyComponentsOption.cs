using System.CommandLine;

namespace Confix.Tool;

public sealed class OnlyComponentsOption : Option<bool>
{
    public OnlyComponentsOption() : base(
        "--only-components",
        "If you specify this option, only the components will be built.")
    {
        IsRequired = false;
        Arity = ArgumentArity.ZeroOrOne;
    }

    public static OnlyComponentsOption Instance { get; } = new();
}
