using System.CommandLine;

namespace Confix.Tool;

public sealed class ReportInputFileOption : Option<FileInfo>
{
    public ReportInputFileOption() : base("--input-file")
    {
        AddAlias("-i");
        AddAlias("--input-file");

        Description = """
        The path to the unencrypted built configuration with all replaced variables. If you specify
        this option, the command will not have to build the configuration first and will be faster.
        """;
    }

    public static ReportInputFileOption Instance { get; } = new();
}