using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Schema;
using ConfiX.Variables;
using Json.Schema;

namespace Confix.Tool.Entities.Components.DotNet;

public sealed class ProjectComposer
    : IProjectComposer
{
    public JsonSchema Compose(IEnumerable<Component> components, IEnumerable<VariablePath> variables)
    {
        var defs = new Dictionary<string, JsonSchema>();
        var properties = new Dictionary<string, JsonSchema>();

        defs["Variable"] = variables.ToVariableStringType();

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

file static class Extensions
{
    public static JsonSchemaBuilder ToVariableStringType(this IEnumerable<VariablePath> variables)
       => new JsonSchemaBuilder()
            .AnyOf(
                variables.ToVariableType(),
                new JsonSchemaBuilder().Type(SchemaValueType.String)
            );

    private static JsonSchemaBuilder ToVariableType(this IEnumerable<VariablePath> variables)
        => new JsonSchemaBuilder()
            .Type(SchemaValueType.String)
            .Enum(variables.Select(v => v.ToString()));
}