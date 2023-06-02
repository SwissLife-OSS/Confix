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
        var type = configuration.Type
            ?? throw new InvalidOperationException("Configuration type is not defined.");

        return new ConfigurationFileDefinition(type, configuration.Value);
    }
}
