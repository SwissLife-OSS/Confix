namespace Confix.Tool.Abstractions;

public sealed record EnvironmentDefinition(
    string Name,
    bool Enabled
)
{
    public static EnvironmentDefinition From(EnvironmentConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration.Name))
        {
            throw new ValidationException("EnvironmentDefinition is invalid")
            {
                Errors = new[] { "Name is null or empty" }
            };
        }

        return new EnvironmentDefinition(
            configuration.Name,
            configuration.Enabled ?? false);
    }
}
