using Confix.Tool;

namespace Confix.Variables;

public sealed record AzureKeyVaultProviderDefinition(
    string Uri
)
{
    public static AzureKeyVaultProviderDefinition From(AzureKeyVaultProviderConfiguration configuration)
    {
        List<string> validationErrors = new();
        if (string.IsNullOrWhiteSpace(configuration.Uri))
        {
            validationErrors.Add("Uri is required");
        }

        if (validationErrors.Any())
        {
            throw new ValidationException("Configuration of AzureKeyVaultProviderConfiguration is invalid")
            { Errors = validationErrors };
        }

        return new(configuration.Uri!);
    }
}