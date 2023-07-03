using System.Text.Json.Nodes;

namespace Confix.Tool.Middlewares.Encryption.Providers.Test;

public sealed class TestEncryptionProvider : IEncryptionProvider
{
    public const string Type = "test";


    public async Task<byte[]> DecryptAsync(byte[] data, CancellationToken cancellationToken)
    {
        return data;
    }

    public async Task<byte[]> EncryptAsync(byte[] data, CancellationToken cancellationToken)
    {
        return data;
    }
}