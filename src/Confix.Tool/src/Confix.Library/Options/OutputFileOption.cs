using System.CommandLine;

namespace Confix.Tool;

public sealed class OutputFileOption : Option<FileInfo>
{
    public OutputFileOption() : base(
        "--output-file",
        "Specifies the output file")
    {
        AddAlias("-o");
        AddAlias("--output-file");
    }

    public static OutputFileOption Instance { get; } = new();
}