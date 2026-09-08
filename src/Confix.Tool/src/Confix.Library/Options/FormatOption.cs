using System.CommandLine;

namespace Confix.Tool;

public sealed class FormatOption : Option<OutputFormat?>
{
    public FormatOption() : base("--format", "-f")
    {
        Description = "Sets the output format";
        this.AcceptOnlyFromAmong("json");
    }

    public static FormatOption Instance { get; } = new();
}

public sealed class FormatOptionWithDefault : Option<OutputFormat?>
{
    public FormatOptionWithDefault() : base("--format", "-f")
    {
        Description = "Sets the output format";
        DefaultValueFactory = _ => OutputFormat.Json;
    }

    public static FormatOptionWithDefault Instance { get; } = new();
}