using System.CommandLine;

namespace Confix.Tool;

public sealed class FormatOption : Option<OutputFormat?>
{
    public FormatOption() : base("--format", "Sets the output format")
    {
        Description = "Sets the output format";
        AddAlias("-f");
        AddAlias("--format");
        this.FromAmong("json");
    }

    public static FormatOption Instance { get; } = new();
}

public sealed class FormatOptionWithDefault : Option<OutputFormat?>
{
    public FormatOptionWithDefault() : base("--format", "Sets the output format")
    {
        Description = "Sets the output format";
        AddAlias("-f");
        AddAlias("--format");
        SetDefaultValue(OutputFormat.Json);
    }

    public static FormatOptionWithDefault Instance { get; } = new();
}
