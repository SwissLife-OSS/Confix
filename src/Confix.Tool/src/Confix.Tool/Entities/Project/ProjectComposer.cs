using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Schema;
using Json.Schema;

namespace Confix.Tool.Entities.Components.DotNet;

public sealed class ProjectComposer
    : IProjectComposer
{
    public JsonSchema Compose(IEnumerable<Component> components)
    {
        var defs = new Dictionary<string, JsonSchema>();
        var properties = new Dictionary<string, JsonSchema>();
        foreach (var componentDefinition in components)
        {
            var prefixedJsonSchema =
                componentDefinition.Schema.PrefixTypes($"{componentDefinition.ComponentName}_");

            if (prefixedJsonSchema.GetDefs() is { } prefixedDefs)
            {
                foreach (var (name, schema) in prefixedDefs)
                {
                    defs[name] = schema;
                }
            }

            defs[componentDefinition.ComponentName] = new JsonSchemaBuilder()
                .Properties(prefixedJsonSchema.GetProperties()!)
                .WithDescription(prefixedJsonSchema.GetDescription())
                .Required(prefixedJsonSchema.GetRequired() ?? Array.Empty<string>())
                .AdditionalProperties(prefixedJsonSchema.GetAdditionalProperties() ?? false)
                .Examples(prefixedJsonSchema.GetExamples() ?? Array.Empty<JsonNode>())
                .Title(prefixedJsonSchema.GetTitle() ?? string.Empty)
                .Build();

            properties[componentDefinition.ComponentName] = new JsonSchemaBuilder()
                .Ref($"#/$defs/{componentDefinition.ComponentName}")
                .Build();
        }

        return new JsonSchemaBuilder()
            .Defs(defs)
            .Properties(properties)
            .Required(properties.Keys.ToArray())
            .Build();
    }
}
