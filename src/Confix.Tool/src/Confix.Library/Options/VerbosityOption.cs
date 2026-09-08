using System.CommandLine;

namespace Confix.Tool;

public sealed class VerbosityOption : Option<Verbosity>
{
    public VerbosityOption() : base("--verbosity")
    {
        Description = "Sets the verbosity level";
        Aliases.Add("-v");
        DefaultValueFactory = _ => Verbosity.Normal;
        this.AcceptOnlyFromAmong("diagnostic", "detailed", "normal", "minimal", "quiet");
    }

    public static VerbosityOption Instance { get; } = new();
}
