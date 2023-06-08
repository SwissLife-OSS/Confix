using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Tool.Schema;
using Json.More;
using Json.Schema;

namespace ConfiX.Variables;

public sealed class JsonVariableRewriter : JsonDocumentRewriter<JsonVariableRewriterContext>
{
    override protected JsonNode Rewrite(JsonValue value, JsonVariableRewriterContext context)
        => value.GetSchemaValueType() switch
        {
            SchemaValueType.String => RewriteVariable((string)value!, context)!,
            _ => value.Deserialize<JsonNode>()!
        };

    private JsonNode RewriteVariable(string key, JsonVariableRewriterContext context)
    {
        if (VariablePath.TryParse(key, out VariablePath? parsed)
            && parsed.HasValue)
        {
            return context.VariableLookup[parsed.Value].Copy()!;
        }

        return key;
    }
}