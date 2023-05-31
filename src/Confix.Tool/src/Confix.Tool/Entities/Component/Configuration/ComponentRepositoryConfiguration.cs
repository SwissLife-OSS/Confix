using System.Text.Json.Nodes;
using Confix.Utilities.Json;

namespace Confix.Tool.Abstractions;

public sealed class ComponentRepositoryConfiguration
{
    private static class FieldNames
    {
        public const string Name = "name";
        public const string Type = "type";
    }

    public ComponentRepositoryConfiguration(string? name, string? type, JsonObject values)
    {
        Name = name;
        Type = type;
        Values = values;
    }

    public string? Name { get; }

    public string? Type { get; }

    public JsonObject Values { get; }

    public static ComponentRepositoryConfiguration Parse(JsonNode node)
    {
        var obj = node.ExpectObject();

        var name = obj.MaybeProperty(FieldNames.Name)?.ExpectValue<string>();

        var type = obj.MaybeProperty(FieldNames.Type)?.ExpectValue<string>();

        return new ComponentRepositoryConfiguration(name, type, obj);
    }
}
