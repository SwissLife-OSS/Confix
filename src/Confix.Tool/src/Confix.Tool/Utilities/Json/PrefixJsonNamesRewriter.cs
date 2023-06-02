using System.Text.Json.Nodes;
using Json.More;
using Json.Schema;

namespace Confix.Tool.Schema;

/// <summary>
/// This rewriter is used to prefix all JsonSchema types with a prefix. The prefix
/// is used to ensure that all types are unique when they are merged together.
/// There are essentially two things that have to be prefixed:
///  - The name of type definitions in $defs
///  - The name in $ref
/// </summary>
public sealed class PrefixJsonNamesRewriter : JsonDocumentRewriter<PrefixJsonNamesContext>
{
    protected override JsonNode Rewrite(JsonObject obj, PrefixJsonNamesContext context)
    {
        var newObject = new JsonObject();
        var prefix = context.Prefix;

        var fieldNames = obj.Select(x => x.Key).ToArray();
        foreach (var field in fieldNames)
        {
            if (obj[field] is not { } value)
            {
                continue;
            }

            if (field is RefKeyword.Name &&
                value is JsonValue refValue &&
                refValue.GetSchemaValueType() is SchemaValueType.String)
            {
                var parts = refValue.GetValue<object>().ToString()!.Split("/");
                parts[^1] = $"{prefix}{parts[^1]}";
                newObject[field] = string.Join("/", parts);
            }
            else if (field is DefsKeyword.Name && value is JsonObject jsonObject)
            {
                var defsObject = new JsonObject();
                var definitionNames = jsonObject.Select(x => x.Key).ToArray();
                foreach (var definitionName in definitionNames)
                {
                    if (jsonObject[definitionName] is { } v)
                    {
                        defsObject[prefix + definitionName] = Rewrite(v, context);
                    }
                }

                newObject[field] = defsObject;
            }
            else
            {
                newObject[field] = Rewrite(value, context);
            }
        }

        return newObject;
    }

    public static PrefixJsonNamesRewriter Instance { get; } = new();
}
