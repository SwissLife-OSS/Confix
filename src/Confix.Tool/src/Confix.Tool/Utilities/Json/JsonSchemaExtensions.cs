using System.Text.Json;
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
}