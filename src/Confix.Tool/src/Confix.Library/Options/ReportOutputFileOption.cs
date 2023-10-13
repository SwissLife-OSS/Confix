using System.CommandLine;

namespace Confix.Tool;

public sealed class ReportOutputFileOption : Option<FileInfo>
{
    public ReportOutputFileOption() : base("--output-file")
    {
        AddAlias("-o");
        AddAlias("--output-file");

        Description =
            "The path to the report file. If not specified, the report will be written to the console.";
    }

    public static ReportOutputFileOption Instance { get; } = new();
}
