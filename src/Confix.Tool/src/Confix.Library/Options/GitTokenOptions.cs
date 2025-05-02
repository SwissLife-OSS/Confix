using System.CommandLine;

namespace Confix.Tool;

internal sealed class GitTokenOptions : Option<string>
{
    public static GitTokenOptions Instance { get; } = new();

    private GitTokenOptions()
        : base("--git-token")
    {
        Arity = ArgumentArity.ZeroOrOne;
        Description = "The token used for git authentication.";
    }
}
