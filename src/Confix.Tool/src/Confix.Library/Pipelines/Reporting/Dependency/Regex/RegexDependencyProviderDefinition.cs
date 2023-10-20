namespace Confix.Tool.Reporting;

public sealed class RegexDependencyProviderDefinition
{
    public RegexDependencyProviderDefinition(string type, string kind, string regex)
    {
        Type = type;
        Kind = kind;
        Regex = regex;
    }

    public string Type { get; }

    public string Kind { get; }

    public string Regex { get; }

    public static RegexDependencyProviderDefinition From(
        RegexDependencyProviderConfiguration configuration)
    {
        var validationErrors = new List<string>();

        if (configuration.Kind is null)
        {
            validationErrors.Add("Kind is not defined.");
        }

        if (configuration.Type is null)
        {
            validationErrors.Add("Type is not defined.");
        }

        if (configuration.Regex is null)
        {
            validationErrors.Add("Regex is not defined.");
        }

        if (validationErrors.Any())
        {
            throw new ValidationException("Invalid component provider configuration")
            {
                Errors = validationErrors
            };
        }

        return new RegexDependencyProviderDefinition(
            configuration.Type!,
            configuration.Kind!,
            configuration.Regex!);
    }
}
