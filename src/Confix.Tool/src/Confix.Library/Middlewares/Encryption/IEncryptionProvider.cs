namespace Confix.Tool.Middlewares.Encryption;

public interface IEncryptionProvider
{
    public static virtual string Type => string.Empty;

    public Task<byte[]> EncryptAsync(byte[] data, CancellationToken cancellationToken);

    public Task<byte[]> DecryptAsync(byte[] data, CancellationToken cancellationToken);
}
