namespace Confix.Tool.Middlewares.Encryption.Providers.AzureKeyvault;

public sealed record AzureKeyVaultEncryptionProviderDefinition(
    string Uri,
    string KeyName,
    string? KeyVersion
)
{
    public static AzureKeyVaultEncryptionProviderDefinition From(
        AzureKeyVaultEncryptionProviderConfiguration configuration)
    {
        List<string> validationErrors = new();
        if (string.IsNullOrWhiteSpace(configuration.Uri))
        {
            validationErrors.Add("Uri is required");
        }

        if (string.IsNullOrWhiteSpace(configuration.KeyName))
        {
            validationErrors.Add("KeyName is required");
        }

        if (validationErrors.Any())
        {
            throw new ValidationException("Configuration of AzureKeyVaultEncryptionProviderConfiguration is invalid")
            { Errors = validationErrors };
        }

        return new(configuration.Uri!, configuration.KeyName!, configuration.KeyVersion);
    }
};
