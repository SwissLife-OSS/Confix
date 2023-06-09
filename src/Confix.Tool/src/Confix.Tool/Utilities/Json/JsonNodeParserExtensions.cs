using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Utilities.Parsing;
using Json.More;

namespace Confix.Utilities.Json;

public static class JsonNodeParserExtensions
{
    public static JsonObject ExpectObject(this JsonNode? node)
        => node switch
        {
            JsonObject obj => obj,

            JsonArray =>
                throw new JsonParseException(node, "Expected object but got array"),

            JsonValue =>
                throw new JsonParseException(node, "Expected object but got value"),

            _ => throw new JsonParseException(JsonNull.SignalNode,
                "Expected object but got null")
        };

    public static JsonObject? MaybeObject(this JsonNode? node)
        => node switch
        {
            JsonObject obj => obj,

            JsonArray =>
                throw new JsonParseException(node, "Expected object but got array"),

            JsonValue =>
                throw new JsonParseException(node, "Expected object but got value"),

            _ => null
        };

    public static JsonArray ExpectArray(this JsonNode? node)
        => node switch
        {
            JsonArray array => array,

            JsonObject =>
                throw new JsonParseException(node, "Expected array but got object"),

            JsonValue =>
                throw new JsonParseException(node, "Expected array but got value"),

            _ => throw new JsonParseException(node, "Expected array but got null")
        };

    public static T ExpectValue<T>(this JsonNode? node)
        => node switch
        {
            JsonValue value => value.TryGetValue(out T? result)
                ? result
                : throw new JsonParseException(node,
                    $"Expected {typeof(T).Name} but got {value.ToJsonString()}"),

            JsonObject => throw new JsonParseException(node,
                $"Expected {typeof(T).Name} but got object"),

            JsonArray => throw new JsonParseException(node,
                $"Expected {typeof(T).Name} but got array"),

            _ => throw new JsonParseException(
                JsonNull.SignalNode,
                $"Expected {typeof(T).Name} but got null")
        };

    public static JsonNode? ExpectProperty(this JsonObject obj, string name)
        => obj.TryGetPropertyValue(name, out var value)
            ? value
            : throw new JsonParseException(obj, $"Expected property '{name}' in object");

    public static JsonNode? MaybeProperty(this JsonObject obj, string name)
        => obj.TryGetPropertyValue(name, out var value) ? value : null;

    private static JsonParseException ToException(this JsonNode node, string message)
    {
        return new(node, message);
    }
}
