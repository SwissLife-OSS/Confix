using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Tool;
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
        if (VariablePath.TryParse(key, out VariablePath? parsed))
        {
            var resolved = context.VariableLookup[parsed.Value with { Suffix = null }];
            if (parsed.Value.Suffix is null)
            {
                return resolved.Copy()!;
            }
            else if (resolved.GetSchemaValueType() == SchemaValueType.String)
            {
                return JsonValue.Create(((string)resolved!) + parsed.Value.Suffix)!;
            }
            else
            {
                throw new ExitException("Cannot append suffix to non-string variable");
            }
        }

        return JsonValue.Create(key)!;
    }
}