namespace Confix.Tool.Middlewares.Encryption;

public interface IEncryptionProvider
{
    public Task<byte[]> EncryptAsync(byte[] data, CancellationToken cancellationToken);
    public Task<byte[]> DecryptAsync(byte[] data, CancellationToken cancellationToken);
}
