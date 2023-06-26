using System.Text.Json.Nodes;

namespace Confix.Tool.Abstractions;

public sealed class ComponentRepositoryDefinition
{
    public ComponentRepositoryDefinition(string name, string type, JsonObject values)
    {
        Name = name;
        Type = type;
        Values = values;
    }

    public string Name { get; }

    public string Type { get; }

    public JsonObject Values { get; }

    public static ComponentRepositoryDefinition From(ComponentRepositoryConfiguration configuration)
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
            throw new ValidationException("Invalid component repository configuration.")
            {
                Errors = validationErrors
            };
        }


        return new ComponentRepositoryDefinition(
            configuration.Name!,
            configuration.Type!,
            configuration.Values);
    }
}
