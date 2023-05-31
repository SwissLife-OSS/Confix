using System.Text.Json.Nodes;
using Confix.Utilities.Json;

namespace Confix.Tool.Abstractions;

public sealed class ComponentProviderConfiguration
{
    private static class FieldNames
    {
        public const string Name = "name";
        public const string Type = "type";
    }

    public ComponentProviderConfiguration(string? name, string? type, JsonObject value)
    {
        Name = name;
        Type = type;
        Value = value;
    }

    public string? Name { get; }

    public string? Type { get; }

    public JsonObject Value { get; }

    public static ComponentProviderConfiguration Parse(JsonNode node)
    {
        var obj = node.ExpectObject();

        var name = obj.MaybeProperty(FieldNames.Name)?.ExpectValue<string>();

        var type = obj.MaybeProperty(FieldNames.Type)?.ExpectValue<string>();

        return new ComponentProviderConfiguration(name, type, obj);
    }
}
