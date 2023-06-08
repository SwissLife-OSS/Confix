using System.Text.Json;
using System.Text.Json.Nodes;

namespace ConfiX.Variables;

public static class JsonParser
{
    public static Dictionary<string, JsonValue?> ParseNode(JsonNode node)
        => node switch
        {
            JsonArray array => new(ParseArray(array)),
            JsonObject obj => new(ParseObject(obj)),
            JsonValue => throw new JsonParserException("Node must be an JsonObject or JsonArray"),
            _ => throw new JsonParserException($"Cant parse type {node.GetType().Name}")
        };

    private static IEnumerable<KeyValuePair<string, JsonValue?>> ParseNodeInternal(JsonNode? node)
        => node switch
        {
        JsonArray array => ParseArray(array),
        JsonObject obj => ParseObject(obj),
        JsonValue value => new[] { KeyValuePair.Create<string, JsonValue?>("", value) },
        null => new[] { KeyValuePair.Create<string, JsonValue?>("", null)},
        _ => throw new JsonParserException($"Cant parse type {node?.GetType().Name}")
        };

    private static IEnumerable<KeyValuePair<string, JsonValue?>> ParseObject(JsonObject jsonObject)
    {
        foreach (KeyValuePair<string, JsonNode?> parentNode in jsonObject)
        {
            foreach (KeyValuePair<string, JsonValue?> item in ParseNodeInternal(parentNode.Value))
            {
                yield return new KeyValuePair<string, JsonValue?>(parentNode.CombineKey(item), item.Value);
            }
        }
    }

    private static IEnumerable<KeyValuePair<string, JsonValue?>> ParseArray(JsonArray jsonArray)
    {
        for (int i = 0; i < jsonArray.Count; i++)
        {
            foreach (KeyValuePair<string, JsonValue?> item in ParseNodeInternal(jsonArray[i]))
            {
                yield return new KeyValuePair<string, JsonValue?>(i.CombineKey(item), item.Value);
            }
        }
    }
}

file static class Extension
{
    public static string CombineKey<P, C>(this KeyValuePair<string, P> parent, KeyValuePair<string, C> child)
    {
        if (string.IsNullOrWhiteSpace(child.Key))
        {
            return parent.Key;
        }
        return $"{parent.Key}.{child.Key}";
    }
    public static string CombineKey<C>(this int index, KeyValuePair<string, C> child)
    {
        if (string.IsNullOrWhiteSpace(child.Key))
        {
            return $"[{index}]";
        }
        return $"[{index}].{child.Key}";
    }
}
