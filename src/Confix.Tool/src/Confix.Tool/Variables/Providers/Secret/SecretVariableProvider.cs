using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using Confix.Tool;

namespace ConfiX.Variables;

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

    public Task<JsonValue> ResolveAsync(string path, CancellationToken cancellationToken)
    {
        byte[] valueToDecrypt = Convert.FromBase64String(path);
        byte[] encryptedValue = Decrypt(valueToDecrypt, _privateKey.Value);
        string decryptedValue = Encoding.UTF8.GetString(encryptedValue);

        return Task.FromResult(JsonNode.Parse(decryptedValue)!.AsValue());
    }

    public async Task<IReadOnlyDictionary<string, JsonValue>> ResolveManyAsync(
        IReadOnlyList<string> paths,
        CancellationToken cancellationToken)
        => new Dictionary<string, JsonValue>(await Task.WhenAll(
            paths.Select(async path => new KeyValuePair<string, JsonValue>(
                path,
                await ResolveAsync(path, cancellationToken)))));

    public Task<string> SetAsync(string path, JsonValue value, CancellationToken cancellationToken)
    {
        string valueToEncrypt = value.ToJsonString();
        byte[] bytesToEncrypt = Encoding.UTF8.GetBytes(valueToEncrypt);
        byte[] encryptedValue = Encrypt(bytesToEncrypt, _publicKey.Value);

        return Task.FromResult(Convert.ToBase64String(encryptedValue));
    }

    private static byte[] Encrypt(ReadOnlySpan<byte> valueToEncrypt, ReadOnlySpan<char> publicKey)
    {
        using RSA rsa = RSA.Create();
        rsa.ImportFromPem(publicKey);
        return rsa.Encrypt(valueToEncrypt, RSAEncryptionPadding.Pkcs1);
    }

    private static byte[] Decrypt(ReadOnlySpan<byte> encryptedValue, ReadOnlySpan<char> privateKey)
    {
        using RSA rsa = RSA.Create();
        rsa.ImportFromPem(privateKey);
        return rsa.Decrypt(encryptedValue, RSAEncryptionPadding.Pkcs1);
    }

    private static char[] GetKey(string? key, string? keyPath)
        => key?.ToCharArray()
            ?? ReadKeyFile(keyPath)
            ?? throw new ExitException("Key or path to key must be set");

    private static char[]? ReadKeyFile(string? path)
        => path is not null ? File.ReadAllText(path).ToCharArray() : null;
}
