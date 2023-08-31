using System.CommandLine;

namespace Confix.Tool;

public sealed class NoRestoreOptions : Option<bool>
{
    public NoRestoreOptions() : base("--no-restore", "Disables restoring of schemas")
    {
        Description = "Disables restoring of schemas";
        SetDefaultValue(false);
    }

    public static NoRestoreOptions Instance { get; } = new();
}
