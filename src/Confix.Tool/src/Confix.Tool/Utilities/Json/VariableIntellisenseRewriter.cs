using System.Text.Json.Nodes;
using Json.More;
using Json.Schema;

namespace Confix.Tool.Schema;

public readonly record struct VariableIntellisenseContext(
    JsonObject VariableReference
);

/// <summary>
/// This rewriter is used to replace all primitives with a type allowing either the primitive
/// or a variable. This is used to allow the user to specify a variable with the support
/// of IntelliSense.
/// </summary>
public sealed class VariableIntellisenseRewriter : JsonDocumentRewriter<VariableIntellisenseContext>
{
    protected override JsonNode Rewrite(JsonObject obj, VariableIntellisenseContext context)
    {
        string? typeName = obj
            .Where(x => x.Key == TypeKeyword.Name)
            .Select(x => (string)x.Value!)
            .SingleOrDefault();

        return typeName switch
        {
            "string" or "number" or "integer" or "boolean" or "array" => RewritePrimitive(obj, context),
            _ => base.Rewrite(obj, context),
        };
    }

    private JsonObject RewritePrimitive(JsonObject obj, VariableIntellisenseContext context)
        => new JsonObject()
        {
            [AnyOfKeyword.Name] = new JsonArray()
                {
                    context.VariableReference.Copy(),
                    base.Rewrite(obj, context),
                }
        };
}
