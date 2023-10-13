using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Tool;
using Confix.Tool.Schema;
using Json.More;
using Json.Schema;

namespace Confix.Variables;

public sealed class JsonVariableRewriter : JsonDocumentRewriter<JsonVariableRewriterContext>
{
    protected override JsonNode Rewrite(JsonValue value, JsonVariableRewriterContext context)
        => value.GetSchemaValueType() switch
        {
            SchemaValueType.String => RewriteVariable((string) value!, context),
            _ => value.Deserialize<JsonNode>()!
        };

    private static JsonNode RewriteVariable(string key, JsonVariableRewriterContext context)
    {
        if (VariablePath.TryParse(key, out var parsed))
        {
            return context.VariableLookup[parsed.Value].Copy()!;
        }

        var replacedString = key.ReplaceVariables(v => context.VariableLookup[v].ToString());
        return JsonValue.Create(replacedString)!;
    }
}