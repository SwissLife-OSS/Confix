using System.Text.Json.Nodes;

namespace Confix.Tool.Schema;

public abstract class JsonDocumentVisitor<TContext>
{
    public virtual void Visit(JsonNode document, TContext context)
    {
        switch (document)
        {
            case JsonArray array:
                Visit(array, context);
                break;

            case JsonObject obj:
                Visit(obj, context);
                break;

            case JsonValue value:
                Visit(value, context);
                break;
        }
    }

    protected virtual void Visit(JsonArray array, TContext context)
    {
        for (var index = 0; index < array.Count; index++)
        {
            if (array[index] is { } e)
            {
                Visit(e, context);
            }
        }
    }

    protected virtual void Visit(JsonObject obj, TContext context)
    {
        foreach (var field in obj)
        {
            if (field.Value is { } elm)
            {
                Visit(elm, context);
            }
        }
    }

    protected virtual void Visit(JsonValue value, TContext context)
    {
    }
}
