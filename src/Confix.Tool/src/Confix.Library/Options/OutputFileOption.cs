using System.CommandLine;

namespace Confix.Tool;

public sealed class OutputFileOption : Option<FileInfo>
{
    public OutputFileOption() : base("--output-file")
    {
        Description = "Specifies the output file";
        Aliases.Add("-o");
    }

    public static OutputFileOption Instance { get; } = new();
}