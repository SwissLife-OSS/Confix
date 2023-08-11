using AES = System.Security.Cryptography.Aes;
using System.Text;
using System.Text.Json.Nodes;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using Confix.Tool.Middlewares.Encryption.Providers.Aes;
using Confix.Utilities.Azure;
using System.Security.Cryptography;

namespace Confix.Tool.Middlewares.Encryption.Providers.AzureKeyvault;

public sealed class AzureKeyVaultEncryptionProvider : IEncryptionProvider
{
    private const char DELIMITER = ';';
    private readonly CryptographyClient _client;

    public AzureKeyVaultEncryptionProvider(JsonNode configuration)
        : this(AzureKeyVaultEncryptionProviderConfiguration.Parse(configuration))
    {
    }

    public AzureKeyVaultEncryptionProvider(
        AzureKeyVaultEncryptionProviderConfiguration configuration)
        : this(AzureKeyVaultEncryptionProviderDefinition.From(configuration))
    {
    }

    public AzureKeyVaultEncryptionProvider(AzureKeyVaultEncryptionProviderDefinition definition)
        : this(new KeyClient(new Uri(definition.Uri), new DefaultAzureCredential())
            .GetCryptographyClient(definition.KeyName, definition.KeyVersion))
    {
    }

    public AzureKeyVaultEncryptionProvider(CryptographyClient client)
    {
        _client = client;
    }

    public static string Type => "AzureKeyVault";

    public async Task<byte[]> EncryptAsync(byte[] data, CancellationToken cancellationToken)
    {
        var aesConfiguration = CreateAesEncryptionProviderDefinition();
        var encryptedData = await EncryptWithAes(aesConfiguration, data, cancellationToken);
        var encryptedAlgorithmConfiguration = await EncryptKeyAndIv(
            aesConfiguration.Key,
            aesConfiguration.IV,
            cancellationToken);
        var contentHash = SHA256.HashData(data);

        return CombineData(
            encryptedData,
            encryptedAlgorithmConfiguration,
            contentHash);
    }

    public async Task<byte[]> DecryptAsync(byte[] data, CancellationToken cancellationToken)
    {
        var (encryptedData, encryptedAlgorithmConfiguration, contentHash) = SplitData(data);
        var (key, iv) = await DecryptKeyAndIv(encryptedAlgorithmConfiguration, cancellationToken);
        var decryptedData = await DecryptWithAes(key, iv, encryptedData, cancellationToken);
        var decryptedContentHash = SHA256.HashData(decryptedData);

        if (!contentHash.SequenceEqual(decryptedContentHash))
        {
            throw new ExitException(
                "Content hash of decrypted data does not match the original hash.");
        }

        return decryptedData;
    }

    private Task<byte[]> EncryptKeyAndIv(
        byte[] key,
        byte[] iv,
        CancellationToken cancellationToken)
    {
        var combinedKeyAndIv = Encoding.UTF8.GetBytes(
            Convert.ToBase64String(key)
            + DELIMITER
            + Convert.ToBase64String(iv));
        return EncryptWithKeyVaultAsync(combinedKeyAndIv, cancellationToken);
    }

    private async Task<(byte[] key, byte[] iv)> DecryptKeyAndIv(
        byte[] encryptedKeyAndIv,
        CancellationToken cancellationToken)
    {
        var decrypted = await DecryptWithKeyVaultAsync(encryptedKeyAndIv, cancellationToken);
        var decryptedKeyAndIv = Encoding.UTF8.GetString(decrypted).Split(DELIMITER);
        return (
            Convert.FromBase64String(decryptedKeyAndIv[0]),
            Convert.FromBase64String(decryptedKeyAndIv[1]));
    }

    private static byte[] CombineData(
        byte[] encryptedData,
        byte[] encryptedAlgorithmData,
        byte[] contentHash)
        => Encoding.UTF8.GetBytes(
            Convert.ToBase64String(encryptedData)
            + DELIMITER
            + Convert.ToBase64String(encryptedAlgorithmData)
            + DELIMITER
            + Convert.ToBase64String(contentHash));

    private static (byte[] encryptedData, byte[] encryptedAlgorithmData, byte[] contentHash)
        SplitData(byte[] data)
    {
        var dataParts = Encoding.UTF8.GetString(data).Split(DELIMITER);
        return (
            Convert.FromBase64String(dataParts[0]),
            Convert.FromBase64String(dataParts[1]),
            Convert.FromBase64String(dataParts[2]));
    }

    private Task<byte[]> DecryptWithKeyVaultAsync(byte[] data, CancellationToken cancellationToken)
        => KeyVaultExtension.HandleKeyVaultException(async () =>
        {
            DecryptResult decrypted = await _client.DecryptAsync(
                EncryptionAlgorithm.RsaOaep256,
                data,
                cancellationToken);
            return decrypted.Plaintext;
        });

    private Task<byte[]> EncryptWithKeyVaultAsync(byte[] data, CancellationToken cancellationToken)
        => KeyVaultExtension.HandleKeyVaultException(async () =>
        {
            EncryptResult encrypted = await _client.EncryptAsync(
                EncryptionAlgorithm.RsaOaep256,
                data,
                cancellationToken);
            return encrypted.Ciphertext;
        });

    private static Task<byte[]> EncryptWithAes(
        AesEncryptionProviderDefinition configuration,
        byte[] data,
        CancellationToken cancellationToken)
    {
        var encryptionProvider = new AesEncryptionProvider(configuration);
        return encryptionProvider.EncryptAsync(data, cancellationToken);
    }

    private static Task<byte[]> DecryptWithAes(
        byte[] key,
        byte[] iv,
        byte[] data,
        CancellationToken cancellationToken)
    {
        var configuration = new AesEncryptionProviderDefinition(key, iv);
        var encryptionProvider = new AesEncryptionProvider(configuration);
        return encryptionProvider.DecryptAsync(data, cancellationToken);
    }

    private static AesEncryptionProviderDefinition CreateAesEncryptionProviderDefinition()
    {
        using AES aes = AES.Create();
        aes.GenerateKey();
        aes.GenerateIV();
        return new(aes.Key, aes.IV);
    }
}
