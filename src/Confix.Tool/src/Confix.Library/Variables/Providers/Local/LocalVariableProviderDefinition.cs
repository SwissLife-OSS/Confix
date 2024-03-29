using Confix.Tool;

namespace Confix.Variables;

public sealed record LocalVariableProviderDefinition(string Path)
{
    public static LocalVariableProviderDefinition From(
        LocalVariableProviderConfiguration configuration)
    {
        var validationErrors = new List<string>();
        if (string.IsNullOrWhiteSpace(configuration.Path))
        {
            validationErrors.Add("Path is required");
        }

        if (validationErrors.Any())
        {
            throw new ValidationException("Invalid configuration for LocalVariableProvider")
            {
                Errors = validationErrors
            };
        }

        return new(configuration.Path!);
    }
}
