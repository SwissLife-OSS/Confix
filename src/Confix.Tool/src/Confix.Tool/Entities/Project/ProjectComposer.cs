using System.Collections.Generic;
using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Schema;
using ConfiX.Variables;
using Json.Schema;

namespace Confix.Tool.Entities.Components.DotNet;

public sealed class ProjectComposer
    : IProjectComposer
{
    private static class WellKnown
    {
        public const string ConfixVariables = "Confix_Variables";
    }

    public JsonSchema Compose(IEnumerable<Component> components, IEnumerable<VariablePath> variables)
    {
        var enumeratedComponents = components.ToArray();
        var defs = GetPrefixedDefinitions(enumeratedComponents);
        var properties = GetProperties(enumeratedComponents);

        var variableType = GetVariableType(variables);

        defs.Add(WellKnown.ConfixVariables, variableType);

        return new JsonSchemaBuilder()
            .Defs(defs)
            .Properties(properties)
            .Required(properties.Keys.ToArray())
            .Build();
    }

    private static Dictionary<string, JsonSchema> GetPrefixedDefinitions(IEnumerable<Component> components)
    {
        Dictionary<string, JsonSchema> defs = new();
        foreach (var componentDefinition in components)
        {
            var prefixedJsonSchema =
                componentDefinition.Schema
                    .PrefixTypes($"{componentDefinition.ComponentName}_")
                    .AddVariableIntellisense(new JsonObject()
                    {
                        [RefKeyword.Name] = $"#/$defs/{WellKnown.ConfixVariables}"
                    });

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
        }

        return defs;
    }

    private static Dictionary<string, JsonSchema> GetProperties(IEnumerable<Component> components)
    {
        Dictionary<string, JsonSchema> properties = new();
        foreach (var componentDefinition in components)
        {
            properties[componentDefinition.ComponentName] = new JsonSchemaBuilder()
                .Ref($"#/$defs/{componentDefinition.ComponentName}")
                .Build();
        }
        return properties;
    }

    private static JsonSchemaBuilder GetVariableType(IEnumerable<VariablePath> variables)
        => new JsonSchemaBuilder()
            .Type(SchemaValueType.String)
            .Enum(variables.Select(v => v.ToString()));
}
