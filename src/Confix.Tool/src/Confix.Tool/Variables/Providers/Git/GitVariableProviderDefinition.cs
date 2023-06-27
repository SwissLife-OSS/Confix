using Confix.Tool;

namespace ConfiX.Variables;

public sealed record GitVariableProviderDefinition(
    string RepositoryUrl,
    string FilePath,
    string[] Arguments
)
{
    public static GitVariableProviderDefinition From(GitVariableProviderConfiguration configuration)
    {
        List<string> validationErrors = new();
        if (string.IsNullOrWhiteSpace(configuration.RepositoryUrl))
        {
            validationErrors.Add("RepositoryUrl is required");
        }
        if (string.IsNullOrWhiteSpace(configuration.FilePath))
        {
            validationErrors.Add("FilePath is required");
        }

        if (validationErrors.Any())
        {
            throw new ValidationException("Configuration of GitVariableProvider is invalid")
            { Errors = validationErrors };
        }

        return new(
            configuration.RepositoryUrl,
            configuration.FilePath,
            configuration.Arguments ?? Array.Empty<string>()
        );
    }
}