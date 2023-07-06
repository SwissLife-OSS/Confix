using Confix.Tool;

namespace ConfiX.Variables;

public sealed record SecretVariableProviderDefinition(
    SecretVariableProviderAlgorithm Algorithm,
    EncryptionPadding Padding,
    string? PublicKey,
    string? PublicKeyPath,
    string? PrivateKey,
    string? PrivateKeyPath,
    string? Password
)
{
    public static SecretVariableProviderDefinition From(SecretVariableProviderConfiguration configuration)
    {
        List<string> validationErrors = new();

        if (!configuration.Algorithm.HasValue)
        {
            validationErrors.Add("Algorithm is required.");
        }
        if (!configuration.Padding.HasValue)
        {
            validationErrors.Add("Padding is required.");
        }
        if (!string.IsNullOrEmpty(configuration.PublicKey)
            && !string.IsNullOrEmpty(configuration.PublicKeyPath))
        {
            validationErrors.Add("PublicKey and PublicKeyPath cannot be specified at the same time.");
        }
        if (!string.IsNullOrEmpty(configuration.PrivateKey)
            && !string.IsNullOrEmpty(configuration.PrivateKeyPath))
        {
            validationErrors.Add("PrivateKey and PrivateKeyPath cannot be specified at the same time.");
        }

        if (validationErrors.Any())
        {
            throw new ValidationException("Invalid configuration for SecretVariableProvider.")
            {
                Errors = validationErrors
            };
        }

        return new(
            configuration.Algorithm!.Value,
            configuration.Padding!.Value,
            configuration.PublicKey,
            configuration.PublicKeyPath,
            configuration.PrivateKey,
            configuration.PrivateKeyPath,
            configuration.Password
        );
    }
}
