using System.CommandLine;

namespace Confix.Tool;

public sealed class NoRestoreOptions : Option<bool>
{
    public NoRestoreOptions() : base("--no-restore")
    {
        Description = "Disables restoring of schemas";
        DefaultValueFactory = _ => false;
    }

    public static NoRestoreOptions Instance { get; } = new();
}
