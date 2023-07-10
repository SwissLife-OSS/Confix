namespace Confix.Tool.Middlewares.Encryption;

public sealed record EncryptionFeature(
    IEncryptionProvider EncryptionProvider
);