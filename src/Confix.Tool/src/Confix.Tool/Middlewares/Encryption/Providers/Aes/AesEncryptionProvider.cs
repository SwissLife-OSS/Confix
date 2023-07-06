using AES = System.Security.Cryptography.Aes;
using System.Text.Json.Nodes;
using System.Security.Cryptography;

namespace Confix.Tool.Middlewares.Encryption.Providers.Aes;

public sealed class AesEncryptionProvider : IEncryptionProvider
{
    private readonly AesEncryptionProviderDefinition _client;

    public AesEncryptionProvider(JsonNode configuration)
        : this(AesEncryptionProviderConfiguration.Parse(configuration))
    { }

    public AesEncryptionProvider(AesEncryptionProviderConfiguration configuration)
        : this(AesEncryptionProviderDefinition.From(configuration))
    { }

    public AesEncryptionProvider(AesEncryptionProviderDefinition client)
    {
        _client = client;
    }

    public const string Type = "Aes";

    public async Task<byte[]> DecryptAsync(byte[] data, CancellationToken cancellationToken)
    {
        using var aes = AES.Create();
        aes.Key = _client.Key;
        aes.IV = _client.IV;
        aes.Padding = PaddingMode.PKCS7;
        aes.Mode = CipherMode.CBC;
        
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, aes.CreateDecryptor(aes.Key, aes.IV), CryptoStreamMode.Write);
        await cs.WriteAsync(data, cancellationToken);
        cs.Close();
        return ms.ToArray();
    }

    public async Task<byte[]> EncryptAsync(byte[] data, CancellationToken cancellationToken)
    {
        using var aes = AES.Create();
        aes.Key = _client.Key;
        aes.IV = _client.IV;
        aes.Padding = PaddingMode.PKCS7;
        aes.Mode = CipherMode.CBC;

        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, aes.CreateEncryptor(aes.Key, aes.IV), CryptoStreamMode.Write);
        await cs.WriteAsync(data, cancellationToken);
        cs.Close();
        return ms.ToArray();
    }
}