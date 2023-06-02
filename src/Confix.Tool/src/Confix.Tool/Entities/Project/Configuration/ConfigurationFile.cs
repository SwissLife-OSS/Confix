using System.Text.Json.Nodes;
using Confix.Utilities.Json;
using Json.Schema;

namespace Confix.Tool.Abstractions;

public sealed class ConfigurationFileConfiguration
{
    private static class FieldNames
    {
        public const string Type = "type";
    }

    public ConfigurationFileConfiguration(string? type, JsonNode value)
    {
        Type = type;
        Value = value;
    }

    public string? Type { get; }

    public JsonNode Value { get; }

    public static ConfigurationFileConfiguration Parse(JsonNode node)
    {
        if (node.GetSchemaValueType() is SchemaValueType.String)
        {
            // TODO const?
            return new ConfigurationFileConfiguration("inline", node);
        }

        var obj = node.ExpectObject();

        var type = obj.MaybeProperty(FieldNames.Type)?.ExpectValue<string>();

        return new ConfigurationFileConfiguration(type, obj);
    }

    public ConfigurationFileConfiguration Merge(ConfigurationFileConfiguration other)
    {
        var type = other.Type ?? Type;
        var value = Value.Merge(other.Value)!;

        return new ConfigurationFileConfiguration(type, value);
    }
}