using System.Text.Json.Nodes;
using Confix.Utilities.Json;

namespace Confix.Tool.Abstractions;

public sealed class ComponentInputConfiguration
{
    private static class FieldNames
    {
        public const string Type = "type";
    }

    public ComponentInputConfiguration(string? type, JsonNode value)
    {
        Type = type;
        Value = value;
    }

    public string? Type { get; }

    public JsonNode Value { get; }

    public static ComponentInputConfiguration Parse(JsonNode element)
    {
        var obj = element.ExpectObject();
        var type = obj.MaybeProperty(FieldNames.Type)?.ExpectValue<string>();

        return new ComponentInputConfiguration(type, obj);
    }

    public ComponentInputConfiguration Merge(ComponentInputConfiguration? other)
    {
        if (other is null)
        {
            return this;
        }

        var type = other.Type ?? Type;
        var value = Value.Merge(other.Value) ?? new JsonObject();

        return new ComponentInputConfiguration(type, value);
    }
}
