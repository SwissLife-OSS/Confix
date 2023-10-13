using System.CommandLine;

namespace Confix.Tool;

public sealed class InputFileOption : Option<FileInfo>
{
    public InputFileOption() : base(
        "--input-file",
        "Specifies the input file")
    {
        AddAlias("-i");
        AddAlias("--input-file");
    }

    public static InputFileOption Instance { get; } = new();
}
