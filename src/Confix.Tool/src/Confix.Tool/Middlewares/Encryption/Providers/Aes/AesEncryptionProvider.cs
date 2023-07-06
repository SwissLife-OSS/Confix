using System.Security.Cryptography;
using System.Text.Json.Nodes;

namespace Confix.Tool.Middlewares.Encryption.Providers.AzureKeyvault;

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

    public Task<byte[]> DecryptAsync(byte[] data, CancellationToken cancellationToken)
    {
        using Aes aes = Aes.Create();
        aes.Key = _client.Key;
        aes.IV = _client.IV;
        return aes.EncryptCbc(data, _client.IV);
    }

    public Task<byte[]> EncryptAsync(byte[] data, CancellationToken cancellationToken)
    {
        using Aes aes = Aes.Create();
        aes.Key = _client.Key;
        aes.IV = _client.IV;
        return aes.DecryptCbc(data, _client.IV);
    }
}