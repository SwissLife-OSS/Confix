using System.CommandLine;

namespace Confix.Tool;

public sealed class VersionOption : Option<string>
{
    public VersionOption() : base("--version")
    {
        Description = "Shows the version information";
        Aliases.Add("-v");
    }

    public static VersionOption Instance { get; } = new();
}
