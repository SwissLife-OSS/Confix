using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Variables;
using Json.Schema;

namespace Confix.Tool.Schema;

public static class JsonSchemaExtensions
{
    public static JsonSchema PrefixTypes(this JsonSchema schema, string prefix)
    {
        var context = new PrefixJsonNamesContext(prefix);
        var visitor = new PrefixJsonNamesRewriter();

        var jsonSchemaAsNode = JsonSerializer.SerializeToNode(schema)!;
        var rewritten = visitor.Rewrite(jsonSchemaAsNode, context);

        return rewritten.Deserialize<JsonSchema>()!;
    }

    public static JsonSchema AddVariableIntellisense(this JsonSchema schema, JsonObject variableRef)
    {
        var context = new VariableIntellisenseContext(
            variableRef
        );

        var jsonSchemaAsNode = JsonSerializer.SerializeToNode(schema)!;
        var rewritten = new VariableIntellisenseRewriter().Rewrite(jsonSchemaAsNode, context);

        return rewritten.Deserialize<JsonSchema>()!;
    }

    public static bool IsArray(this JsonSchema schema)
        => schema.GetJsonType() is SchemaValueType.Array ||
            (schema.GetAnyOf()?.Any(x => x.IsArray()) ?? false);
}
