using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using Confix.Tool;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;

namespace ConfiX.Variables;

public sealed class SecretVariableProvider : IVariableProvider
{
    private readonly SecretVariableProviderConfiguration _configuration;
    private readonly Lazy<string> _privateKey;
    private readonly Lazy<string> _publicKey;

    public SecretVariableProvider(JsonNode configuration)
        : this(SecretVariableProviderConfiguration.Parse(configuration))
    {
    }

    public SecretVariableProvider(SecretVariableProviderConfiguration configuration)
    {
        _configuration = configuration;
        _privateKey = new Lazy<string>(()
            => GetKey(_configuration.PrivateKey, _configuration.PrivateKeyPath));
        _publicKey = new Lazy<string>(()
            => GetKey(_configuration.PublicKey, _configuration.PublicKeyPath));
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

    private byte[] Encrypt(byte[] valueToEncrypt, string publicKey)
    {
        using RSA rsa = RSA.Create();
        rsa.ImportFromPem(publicKey);
        return rsa.Encrypt(valueToEncrypt, Padding);
    }

    private byte[] Decrypt(ReadOnlySpan<byte> encryptedValue, string privateKey)
    {
        using RSA rsa = RSA.Create();
        if (_configuration.Password is { } password)
        {
            privateKey = DecryptPrivateKey(privateKey, password.ToCharArray());
        }

        rsa.ImportFromPem(privateKey);

        return rsa.Decrypt(encryptedValue, Padding);
    }

    private static string DecryptPrivateKey(string encryptedPrivateKey, char[] password)
    {
        try
        {
            using var textReader = new StringReader(encryptedPrivateKey);
            var pemReader = new PemReader(textReader, new PasswordFinder(password));

            var keyPair = (AsymmetricCipherKeyPair) pemReader.ReadObject();
            var privateKey = (RsaPrivateCrtKeyParameters) keyPair.Private;

            var textWriter = new StringWriter();
            var pemWriter = new PemWriter(textWriter);
            pemWriter.WriteObject(privateKey);

            return textWriter.ToString();
        }
        catch (Exception ex)
        {
            throw new ExitException(
                "Could not decrypt private key. Please check your password.",
                ex);
        }
    }

    private RSAEncryptionPadding Padding => _configuration.Padding switch
    {
        EncryptionPadding.OaepSHA256 => RSAEncryptionPadding.OaepSHA256,
        EncryptionPadding.OaepSHA512 => RSAEncryptionPadding.OaepSHA512,
        EncryptionPadding.Pkcs1 => RSAEncryptionPadding.Pkcs1,
        _ => throw new ArgumentException("Invalid padding", nameof(_configuration.Padding))
    };

    private static string GetKey(string? key, string? keyPath)
        => key
            ?? ReadKeyFile(keyPath)
            ?? throw new ExitException("Key or path to key must be set");

    private static string? ReadKeyFile(string? path)
        => path is not null ? File.ReadAllText(path) : null;
}

file sealed class PasswordFinder : IPasswordFinder
{
    private readonly char[] _chars;

    public PasswordFinder(char[] chars)
    {
        _chars = chars;
    }

    /// <inheritdoc />
    public char[] GetPassword()
    {
        return _chars;
    }
}
