using System.Text.Json.Nodes;

namespace Confix.Tool.Abstractions;

public sealed class ConfigurationFileDefinition
{
    public string Type { get; }

    public JsonNode Value { get; }

    public ConfigurationFileDefinition(string type, JsonNode value)
    {
        Type = type;
        Value = value;
    }

    public static ConfigurationFileDefinition From(ConfigurationFileConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration.Type))
        {
            throw new ValidationException("ConfigurationFileDefinition is invalid.")
            {
                Errors = new[] { "Type is required." }
            };
        }

        return new ConfigurationFileDefinition(configuration.Type, configuration.Value);
    }
}
