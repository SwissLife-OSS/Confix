namespace Confix.Tool.Middlewares.Encryption;

public interface IEncryptionProviderFactory
{
    IEncryptionProvider CreateProvider(EncryptionProviderConfiguration providerConfiguration);
}
