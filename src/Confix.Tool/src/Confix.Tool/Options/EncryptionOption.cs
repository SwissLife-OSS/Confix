using System.CommandLine;

namespace Confix.Tool;

public sealed class EncryptionOption : Option<bool>
{
    public EncryptionOption() : base(
        "--encrypt",
        "Encrypt the output file")
    {
    }

    public static EncryptionOption Instance { get; } = new();
}