using System.Text.Json.Nodes;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using Confix.Utilities.Azure;

namespace Confix.Tool.Middlewares.Encryption.Providers.AzureKeyvault;

public sealed class AzureKeyVaultEncryptionProvider : IEncryptionProvider
{
    private readonly CryptographyClient _client;

    public AzureKeyVaultEncryptionProvider(JsonNode configuration)
        : this(AzureKeyVaultEncryptionProviderConfiguration.Parse(configuration))
    { }

    public AzureKeyVaultEncryptionProvider(AzureKeyVaultEncryptionProviderConfiguration configuration)
        : this(AzureKeyVaultEncryptionProviderDefinition.From(configuration))
    { }

    public AzureKeyVaultEncryptionProvider(AzureKeyVaultEncryptionProviderDefinition definition)
        : this(new KeyClient(new Uri(definition.Uri), new DefaultAzureCredential())
            .GetCryptographyClient(definition.KeyName, definition.KeyVersion))
    { }

    public AzureKeyVaultEncryptionProvider(CryptographyClient client)
    {
        _client = client;
    }

    public const string Type = "AzureKeyVault";

    public Task<byte[]> DecryptAsync(byte[] data, CancellationToken cancellationToken)
    => KeyVaultExtension.HandleKeyVaultException(async () =>
    {
        DecryptResult decrypted = await _client.DecryptAsync(
            EncryptionAlgorithm.RsaOaep256,
            data,
            cancellationToken);
        return decrypted.Plaintext;
    });

    public Task<byte[]> EncryptAsync(byte[] data, CancellationToken cancellationToken)
    => KeyVaultExtension.HandleKeyVaultException(async () =>
    {
        EncryptResult encrypted = await _client.EncryptAsync(
            EncryptionAlgorithm.RsaOaep256,
            data,
            cancellationToken);
        return encrypted.Ciphertext;
    });
}