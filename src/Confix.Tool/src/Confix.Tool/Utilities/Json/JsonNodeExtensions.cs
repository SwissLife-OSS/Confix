using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using Json.More;
using Json.Schema;

namespace Confix.Utilities.Json;

public static class JsonNodeExtensions
{
    public static bool TryGetNonNullPropertyValue(
        this JsonObject obj,
        string propertyName,
        [NotNullWhen(true)] out JsonNode? value)
    {
        if (obj.TryGetPropertyValue(propertyName, out var node) && node is not null)
        {
            value = node;
            return true;
        }

        value = null;
        return false;
    }

    public static JsonNode? Merge(this JsonNode? source, JsonNode? node)
        => (source, node) switch
        {
            (null, null) => null,

            (not null, null) => source,

            (null, not null) => node,

            (JsonObject sourceObject, JsonObject nodeObject) => Merge(sourceObject, nodeObject),

            (JsonArray sourceArray, JsonArray nodeArray) => Merge(sourceArray, nodeArray),

            (JsonValue sourceValue, JsonValue nodeValue) =>
                throw new InvalidOperationException($"""
                    Cannot merge values:
                    Source: {sourceValue.ToJsonString()}
                    Node: {nodeValue.ToJsonString()}
                """),
            _ => throw new InvalidOperationException($"""
                    Cannot merge nodes of different types:
                    Source: {source.GetSchemaValueType()}
                    Node: {node.GetSchemaValueType()}
                """)
        };

    private static JsonArray Merge(this JsonArray source, JsonArray node)
    {
        var array = new JsonNode?[source.Count + node.Count];

        for (var i = 0; i < source.Count; i++)
        {
            array[i] = source[i]?.Copy();
        }
        
        for (var i = 0; i < node.Count; i++)
        {
            array[i + source.Count] = node[i]?.Copy();
        }

        return new JsonArray(array);
    }

    private static JsonObject Merge(JsonObject source, JsonObject node)
    {
        var obj = new JsonObject();

        foreach (var (key, value) in source)
        {
            obj[key] = value.Copy();
        }

        foreach (var (key, value) in node)
        {
            obj[key] = value.Copy();
        }

        return obj;
    }
}
