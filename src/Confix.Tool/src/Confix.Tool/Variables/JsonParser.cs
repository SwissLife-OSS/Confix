using System.Text.Json;
using System.Text.Json.Nodes;

namespace Confix.Variables;

public static class JsonParser
{
    public static Dictionary<string, JsonNode?> ParseNode(JsonNode node)
        => node switch
        {
            JsonArray array => ParseArray(array).ToDictionary(),
            JsonObject obj => ParseObject(obj).ToDictionary(),
            JsonValue => throw new JsonParserException("Node must be an JsonObject or JsonArray"),
            _ => throw new JsonParserException($"Cant parse type {node.GetType().Name}")
        };

    private static IEnumerable<KeyValuePair<string, JsonNode?>> ParseNodeInternal(JsonNode? node)
        => node switch
        {
            JsonArray array => ParseArray(array),
            JsonObject obj => ParseObject(obj),
            JsonValue value => new[] { KeyValuePair.Create<string, JsonNode?>("", value) },
            null => new[] { KeyValuePair.Create<string, JsonNode?>("", null) },
            _ => throw new JsonParserException($"Cant parse type {node?.GetType().Name}")
        };

    private static IEnumerable<KeyValuePair<string, JsonNode?>> ParseObject(JsonObject jsonObject)
    {
        yield return new KeyValuePair<string, JsonNode?>("", jsonObject);
        foreach (KeyValuePair<string, JsonNode?> parentNode in jsonObject)
        {
            foreach (KeyValuePair<string, JsonNode?> item in ParseNodeInternal(parentNode.Value))
            {
                yield return new KeyValuePair<string, JsonNode?>(parentNode.CombineKey(item), item.Value);
            }
        }
    }

    private static IEnumerable<KeyValuePair<string, JsonNode?>> ParseArray(JsonArray jsonArray)
    {
        yield return new KeyValuePair<string, JsonNode?>("", jsonArray);
        for (int i = 0; i < jsonArray.Count; i++)
        {
            foreach (KeyValuePair<string, JsonNode?> item in ParseNodeInternal(jsonArray[i]))
            {
                yield return new KeyValuePair<string, JsonNode?>(i.CombineKey(item), item.Value);
            }
        }
    }


}

file static class Extension
{
    public static Dictionary<string, JsonNode?> ToDictionary(
        this IEnumerable<KeyValuePair<string, JsonNode?>> items)
    {
        var result = new Dictionary<string, JsonNode?>();
        foreach (KeyValuePair<string, JsonNode?> item in items)
        {
            if (string.IsNullOrEmpty(item.Key))
            {
                continue;
            }

            if (result.ContainsKey(item.Key))
            {
                throw new JsonParserException($"Duplicate key {item.Key}");
            }

            result.Add(item.Key, item.Value);
        }
        return result;
    }

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
