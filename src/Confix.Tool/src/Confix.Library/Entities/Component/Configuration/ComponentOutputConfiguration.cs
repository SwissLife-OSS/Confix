using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Utilities.Json;

namespace Confix.Tool.Abstractions;

public sealed class ComponentOutputConfiguration
{
    public static class FieldNames
    {
        public const string Type = "type";
    }

    public ComponentOutputConfiguration(string? type, JsonObject value)
    {
        Type = type;
        Value = value;
    }

    public string? Type { get; }

    public JsonObject Value { get; }

    public static ComponentOutputConfiguration Parse(JsonNode element)
    {
        var obj = element.ExpectObject();
        var type = obj.MaybeProperty(FieldNames.Type)?.ExpectValue<string>();

        return new ComponentOutputConfiguration(type, obj);
    }

    public ComponentOutputConfiguration Merge(ComponentOutputConfiguration? other)
    {
        if (other is null)
        {
            return this;
        }

        var type = other.Type ?? Type;
        var value = Value.Merge(other.Value) as JsonObject ?? new JsonObject();

        return new ComponentOutputConfiguration(type, value);
    }
}
