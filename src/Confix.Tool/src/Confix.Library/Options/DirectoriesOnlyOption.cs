using System.CommandLine;

namespace Confix.Tool;

public sealed class DirectoriesOnlyOption : Option<bool>
{
    public DirectoriesOnlyOption() : base(
        "--directories",
        "Return only the parent directory paths instead of the full file paths")
    {
        Description = "Return only the parent directory paths instead of the full file paths";
        AddAlias("--dirs");
        SetDefaultValue(false);
    }

    public static DirectoriesOnlyOption Instance { get; } = new();
}
