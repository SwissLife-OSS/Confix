namespace Confix.Tool.Entities.Components.Git;

public sealed record GitComponentProviderDefinition(
    string Name,
    string RepositoryUrl,
    string Path,
    string[] Arguments)
{
    public static GitComponentProviderDefinition From(
        GitComponentProviderConfiguration configuration)
    {
        var validationErrors = new List<string>();
        if (string.IsNullOrWhiteSpace(configuration.Name))
        {
            validationErrors.Add("Name is required");
        }

        if (string.IsNullOrWhiteSpace(configuration.RepositoryUrl))
        {
            validationErrors.Add("RepositoryUrl is required");
        }

        if (string.IsNullOrWhiteSpace(configuration.Path))
        {
            validationErrors.Add("FilePath is required");
        }

        if (validationErrors.Any())
        {
            throw new ValidationException("Configuration of GitComponentProvider is invalid")
                { Errors = validationErrors };
        }

        return new(
            configuration.Name!,
            configuration.RepositoryUrl!,
            configuration.Path!,
            configuration.Arguments ?? Array.Empty<string>());
    }
}
