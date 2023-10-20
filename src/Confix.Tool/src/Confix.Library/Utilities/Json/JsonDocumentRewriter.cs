using System.Text.Json;
using System.Text.Json.Nodes;

namespace Confix.Tool.Schema;

public abstract class JsonDocumentRewriter<TContext>
{
    public virtual JsonNode Rewrite(JsonNode document, TContext context)
        => document switch
        {
            JsonArray array => Rewrite(array, context),
            JsonObject obj => Rewrite(obj, context),
            JsonValue value => Rewrite(value, context),
            _ => document
        };

    protected virtual JsonNode Rewrite(JsonArray array, TContext context)
    {
        var newArray = new JsonNode?[array.Count];

        for (var index = 0; index < array.Count; index++)
        {
            if (array[index] is { } e)
            {
                newArray[index] = Rewrite(e, context);
            }
            else
            {
                newArray[index] = null;
            }
        }

        return new JsonArray(newArray);
    }

    protected virtual JsonNode Rewrite(JsonObject obj, TContext context)
    {
        var newObject = new JsonObject();
        var keys = obj.Select(x => x.Key).ToArray();

        foreach (var field in keys)
        {
            newObject[field] = obj[field] is { } elm
                ? Rewrite(elm, context)
                : null;
        }

        return newObject;
    }

    protected virtual JsonNode Rewrite(JsonValue value, TContext context)
    {
        return value.Deserialize<JsonNode>()!;
    }
}