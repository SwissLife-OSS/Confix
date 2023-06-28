using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using Confix.Tool;

namespace Confix.Variables;

public sealed class SecretVariableProvider : IVariableProvider
{
    private readonly SecretVariableProviderConfiguration _configuration;
    private readonly Lazy<char[]> _privateKey;
    private readonly Lazy<char[]> _publicKey;

    public SecretVariableProvider(JsonNode configuration)
       : this(SecretVariableProviderConfiguration.Parse(configuration))
    { }

    public SecretVariableProvider(SecretVariableProviderConfiguration configuration)
    {
        _configuration = configuration;
        _privateKey = new Lazy<char[]>(() => GetKey(_configuration.PrivateKey, _configuration.PrivateKeyPath));
        _publicKey = new Lazy<char[]>(() => GetKey(_configuration.PublicKey, _configuration.PublicKeyPath));
    }

    public Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());

    public Task<JsonNode> ResolveAsync(string path, CancellationToken cancellationToken)
    {
        byte[] valueToDecrypt = Convert.FromBase64String(path);
        byte[] encryptedValue = Decrypt(valueToDecrypt, _privateKey.Value);
        string decryptedValue = Encoding.UTF8.GetString(encryptedValue);

        return Task.FromResult(JsonNode.Parse(decryptedValue)!);
    }

    public Task<IReadOnlyDictionary<string, JsonNode>> ResolveManyAsync(
        IReadOnlyList<string> paths,
        CancellationToken cancellationToken)
        => paths.ResolveMany(ResolveAsync, cancellationToken);

    public Task<string> SetAsync(string path, JsonNode value, CancellationToken cancellationToken)
    {
        string valueToEncrypt = value.ToJsonString();
        byte[] bytesToEncrypt = Encoding.UTF8.GetBytes(valueToEncrypt);
        byte[] encryptedValue = Encrypt(bytesToEncrypt, _publicKey.Value);

        return Task.FromResult(Convert.ToBase64String(encryptedValue));
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    private byte[] Encrypt(ReadOnlySpan<byte> valueToEncrypt, ReadOnlySpan<char> publicKey)
    {
        using RSA rsa = RSA.Create();
        rsa.ImportFromPem(publicKey);
        return rsa.Encrypt(valueToEncrypt, Padding);
    }

    private byte[] Decrypt(ReadOnlySpan<byte> encryptedValue, ReadOnlySpan<char> privateKey)
    {
        using RSA rsa = RSA.Create();
        rsa.ImportFromPem(privateKey);
        return rsa.Decrypt(encryptedValue, Padding);
    }

    private RSAEncryptionPadding Padding => _configuration.Padding switch
    {
        EncryptionPadding.OaepSHA256 => RSAEncryptionPadding.OaepSHA256,
        EncryptionPadding.OaepSHA512 => RSAEncryptionPadding.OaepSHA512,
        EncryptionPadding.Pkcs1 => RSAEncryptionPadding.Pkcs1,
        _ => throw new ArgumentException("Invalid padding", nameof(_configuration.Padding))
    };

    private static char[] GetKey(string? key, string? keyPath)
        => key?.ToCharArray()
            ?? ReadKeyFile(keyPath)
            ?? throw new ExitException("Key or path to key must be set");

    private static char[]? ReadKeyFile(string? path)
        => path is not null ? File.ReadAllText(path).ToCharArray() : null;
}
