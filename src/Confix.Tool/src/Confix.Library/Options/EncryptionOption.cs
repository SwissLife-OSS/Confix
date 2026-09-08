using System.CommandLine;

namespace Confix.Tool;

public sealed class EncryptionOption : Option<bool>
{
    public EncryptionOption() : base("--encrypt")
    {
        Description = "Encrypt the output file";
    }

    public static EncryptionOption Instance { get; } = new();
}