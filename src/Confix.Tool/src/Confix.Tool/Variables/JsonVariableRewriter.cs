using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;

namespace ConfiX.Variables;

public class JsonVariableRewriter
{
    private readonly Dictionary<VariablePath, string> _variableLookup;

    public JsonVariableRewriter(Dictionary<VariablePath, string> variableLookup)
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

    public JsonArray RewriteArray(JsonArray node)
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
            JsonValueKind.String => RewriteVariable((string?)value),
            _ => value.Copy()
        };

    private JsonValue? RewriteVariable(string? key)
    {
        if (
            key is not null
            && VariablePath.TryParse(key, out VariablePath? parsed)
            && parsed.HasValue)
        {
            // TODO
            /* 
                would be nice to actually put a number into the string 
                if the resolved value is a number (or bool).
                we could just try to parse the resolved value and see if it is a number 
                (probably a bad idea)
            */
            return JsonValue.Create(_variableLookup[parsed.Value]);
        }

        return JsonValue.Create(key);
    }
}