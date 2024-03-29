using System.CommandLine;

namespace Confix.Tool;

public sealed class VerbosityOption : Option<Verbosity>
{
    public VerbosityOption() : base(
        "--verbosity",
        "Sets the verbosity level")
    {
        Description = "Sets the verbosity level";
        AddAlias("-v");
        AddAlias("--verbosity");
        SetDefaultValue(Verbosity.Normal);
        this.FromAmong("diagnostic", "detailed", "normal", "minimal", "quiet");
    }

    public static VerbosityOption Instance { get; } = new();
}
