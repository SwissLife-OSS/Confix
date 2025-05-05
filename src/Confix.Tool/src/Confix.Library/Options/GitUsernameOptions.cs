using System.CommandLine;

namespace Confix.Tool;

internal sealed class GitUsernameOptions : Option<string>
{
    public static GitUsernameOptions Instance { get; } = new();

    private GitUsernameOptions()
        : base("--git-username")
    {
        Arity = ArgumentArity.ZeroOrOne;
        Description = "The username used for git authentication.";
    }
}
