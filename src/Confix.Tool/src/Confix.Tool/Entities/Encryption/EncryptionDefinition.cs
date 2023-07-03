namespace Confix.Tool.Abstractions;

public record EncryptionDefinition(
    EncryptionProviderDefinition Provider
)
{
    public static EncryptionDefinition From(EncryptionConfiguration configuration)
    {
        List<string> validationErrors = new();

        if (configuration.Provider is null)
        {
            validationErrors.Add("Provider is required.");
        }
        if (validationErrors.Any())
        {
            throw new ValidationException("Encryption configuration is invalid.")
            {
                Errors = validationErrors
            };
        }

        var provider = EncryptionProviderDefinition.From(configuration.Provider!);

        return new EncryptionDefinition(provider);
    }
};