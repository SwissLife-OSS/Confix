using Confix.Tool;

namespace Confix.Variables;

public sealed record OnePasswordProviderDefinition(
    string Vault,
    string? ServiceAccountToken,
    string? Account
)
{
    public static OnePasswordProviderDefinition From(
        OnePasswordProviderConfiguration configuration)
    {
        List<string> validationErrors = new();
        if (string.IsNullOrWhiteSpace(configuration.Vault))
        {
            validationErrors.Add("Vault is required");
        }

        if (validationErrors.Any())
        {
            throw new ValidationException(
                "Configuration of OnePasswordProviderConfiguration is invalid")
            { Errors = validationErrors };
        }

        return new(
            configuration.Vault!,
            configuration.ServiceAccountToken,
            configuration.Account
        );
    }
}
