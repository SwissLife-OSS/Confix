using System.CommandLine;

namespace Confix.Tool;

public sealed class VersionOption : Option<string>
{
    public VersionOption() : base("--version", "Specify the version")
    {
        Description = "Shows the version information";
        AddAlias("-v");
        AddAlias("--version");
    }

    public static VersionOption Instance { get; } = new();
}
