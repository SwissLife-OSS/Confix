using System.CommandLine;

namespace Confix.Tool.Pipelines.Encryption;

internal sealed class OutputFileArgument : Argument<FileInfo?>
{
    public static OutputFileArgument Instance { get; } = new();

    private OutputFileArgument()
        : base("out-file")
    {
        Arity = ArgumentArity.ZeroOrOne;
        Description = """
            The file to write the encrypted or decrypted data to. 
            If not provided the input file will be overwritten. 
            Existing files will be overwritten.
            """;
    }
}