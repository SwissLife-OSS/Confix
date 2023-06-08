using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;

namespace ConfiX.Variables;

public sealed class JsonVariableRewriter
{
    private readonly IReadOnlyDictionary<VariablePath, JsonNode> _variableLookup;

    public JsonVariableRewriter(IReadOnlyDictionary<VariablePath, JsonNode> variableLookup)
    {
        _variableLookup = variableLookup;
    }

    public JsonNode? Rewrite(JsonNode? node)
        => node switch
        {
            null => null,
            JsonArray array => RewriteArray(array),
            JsonObject obj => RewriteObject(obj),
            JsonValue val => RewriteValue(val),
            _ => throw new JsonParserException($"Cant rewrite type {node.GetType().Name}")
        };

    private JsonObject RewriteObject(JsonObject node)
    {
        JsonObject result = new();
        foreach (KeyValuePair<string, JsonNode?> x in node)
        {
            result.Add(x.Key, Rewrite(x.Value));
        }

        return result;
    }

    private JsonArray RewriteArray(JsonArray node)
    {
        JsonArray result = new();

        foreach (JsonNode? x in node)
        {
            result.Add(Rewrite(x));
        }

        return result;
    }

    private JsonNode? RewriteValue(JsonValue value)
        => value.GetValue<JsonElement>().ValueKind switch
        {
            JsonValueKind.String => RewriteVariable((string)value!),
            _ => value.Copy()
        };

    private JsonNode? RewriteVariable(string key)
    {
        if (VariablePath.TryParse(key, out VariablePath? parsed)
            && parsed.HasValue)
        {
            return _variableLookup[parsed.Value].Copy();
        }

        return key;
    }
}