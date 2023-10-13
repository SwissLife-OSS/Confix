using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Tool.Schema;
using Json.More;
using Json.Schema;

namespace Confix.Variables;

public sealed class JsonVariableExtractor : JsonDocumentRewriter<JsonVariableExtractorContext>
{
    protected override JsonNode Rewrite(JsonValue value, JsonVariableExtractorContext context)
        => value.GetSchemaValueType() switch
        {
            SchemaValueType.String => RewriteVariable((string) value!, context),
            _ => value.Deserialize<JsonNode>()!
        };

    private static JsonNode RewriteVariable(string key, JsonVariableExtractorContext context)
    {
        if (VariablePath.TryParse(key, out var parsed))
        {
            return context.VariableLookup[parsed.Value].Copy()!;
        }

        var replacedString = key.ReplaceVariables(v => context.VariableLookup[v].ToString());
        return JsonValue.Create(replacedString)!;
    }
}

public sealed record JsonVariableExtractorContext(
    IReadOnlyDictionary<VariablePath, JsonNode> VariableLookup);
