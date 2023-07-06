namespace Confix.Tool.Middlewares.Encryption.Providers.AzureKeyvault;

public sealed record AesEncryptionProviderDefinition(
    byte[] Key,
    byte[] IV)
{
    public static AesEncryptionProviderDefinition From(
        AesEncryptionProviderConfiguration configuration)
    {
        List<string> validationErrors = new();
        if (string.IsNullOrWhiteSpace(configuration.Key))
        {
            validationErrors.Add("Key is required");
        }
        if (string.IsNullOrWhiteSpace(configuration.IV))
        {
            validationErrors.Add("IV is required");
        }

        if (validationErrors.Any())
        {
            throw new ValidationException("Configuration of AesEncryptionProviderDefinition is invalid")
            { Errors = validationErrors };
        }

        return new(
            Convert.FromBase64String(configuration.Key!),
            Convert.FromBase64String(configuration.IV!)
        );
    }
};
