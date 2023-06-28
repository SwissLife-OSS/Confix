using System.Text.Json.Nodes;

namespace Confix.Tool.Abstractions;

public sealed class ComponentProviderDefinition
{
    public string Name { get; }

    public string Type { get; }

    public JsonObject Value { get; }

    public ComponentProviderDefinition(string name, string type, JsonObject values)
    {
        Name = name;
        Type = type;
        Value = values;
    }

    public static ComponentProviderDefinition From(ComponentProviderConfiguration configuration)
    {
        List<string> validationErrors = new();
        if (configuration.Name is null)
        {
            validationErrors.Add("Name is not defined.");
        }
        if (configuration.Type is null)
        {
            validationErrors.Add("Type is not defined.");
        }

        if (validationErrors.Any())
        {
            throw new ValidationException("Invalid component provider configuration")
            {
                Errors = validationErrors
            };
        }

        return new ComponentProviderDefinition(
            configuration.Name!,
            configuration.Type!,
            configuration.Values);
    }
}
