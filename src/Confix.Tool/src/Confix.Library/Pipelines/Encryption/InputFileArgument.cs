using System.CommandLine;

namespace Confix.Tool.Pipelines.Encryption;

internal sealed class InputFileArgument : Argument<FileInfo>
{
    public static InputFileArgument Instance { get; } = new();

    private InputFileArgument()
        : base("input-file")
    {
        Arity = ArgumentArity.ExactlyOne;
        Description = "The path to the file to encrypt or decrypt.";
    }
}
