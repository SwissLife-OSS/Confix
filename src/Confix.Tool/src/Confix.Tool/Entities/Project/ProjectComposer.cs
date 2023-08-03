using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Schema;
using Confix.Utilities.Json;
using Confix.Variables;
using Json.Schema;

namespace Confix.Tool.Entities.Components.DotNet;

public sealed class ProjectComposer
    : IProjectComposer
{
    public static class References
    {
        public const string ConfixVariables = "Confix_Variables";

        public static class Urls
        {
            public const string ConfixVariables = $"#/$defs/{References.ConfixVariables}";
        }
    }

    public JsonSchema Compose(
        IEnumerable<Component> components,
        IEnumerable<VariablePath> variables)
    {
        var enumeratedComponents = components.ToArray();
        var defs = GetPrefixedDefinitions(enumeratedComponents);
        var properties = GetProperties(enumeratedComponents);

        var variableType = GetVariableType(variables);

        defs.Add(References.ConfixVariables, variableType);

        return new JsonSchemaBuilder()
            .Defs(defs)
            .Properties(properties)
            .Required(properties.Keys.ToArray())
            .Build();
    }

    private static Dictionary<string, JsonSchema> GetPrefixedDefinitions(
        IEnumerable<Component> components)
    {
        Dictionary<string, JsonSchema> defs = new();
        foreach (var componentDefinition in components)
        {
            var prefixedJsonSchema =
                componentDefinition.Schema
                    .PrefixTypes($"{componentDefinition.ComponentName}_")
                    .AddVariableIntellisense(new JsonObject()
                    {
                        [RefKeyword.Name] = References.Urls.ConfixVariables
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
                .Title(prefixedJsonSchema.GetTitle() ?? string.Empty);
        }

        return defs;
    }

    private static Dictionary<string, JsonSchema> GetProperties(IEnumerable<Component> components)
        => SchemaNode
            .FromComponents(components)
            .Children
            .ToDictionary(x => x.Key, x => x.Value.BuildSchema());

    private static JsonSchemaBuilder GetVariableType(IEnumerable<VariablePath> variables)
        => new JsonSchemaBuilder()
            .Type(SchemaValueType.String)
            .Enum(variables.Select(v => v.ToString()));
}

file class SchemaNode
{
    private readonly Dictionary<string, SchemaNode> _children = new();
    private readonly List<JsonSchemaBuilder> _schemas = new();

    public SchemaNode(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public IReadOnlyDictionary<string, SchemaNode> Children => _children;

    public IReadOnlyList<JsonSchemaBuilder> Schemas => _schemas;

    public JsonSchema BuildSchema()
    {
        JsonSchemaBuilder? schema;
        switch (Schemas.Count)
        {
            case > 1:
                schema = new();
                schema.AnyOf(Schemas.Select(x => x.Build()));
                break;

            case 1:
                schema = Schemas[0];
                break;

            default:
                schema = new JsonSchemaBuilder();
                break;
        }

        foreach (var (name, child) in Children)
        {
            schema.Properties((name, child.BuildSchema()));
            schema.Required(name);
        }

        return schema.Build();
    }

    public static SchemaNode FromComponents(IEnumerable<Component> components)
    {
        var root = new SchemaNode("Root");
        foreach (var componentDefinition in components)
        {
            foreach (var mountingPoint in componentDefinition.MountingPoints)
            {
                var mountingPoints = mountingPoint.Split('.');

                var current = root;
                for (var i = 0; i < mountingPoints.Length; i++)
                {
                    var mountingPointName = mountingPoints[i];
                    if (!current._children.TryGetValue(mountingPointName, out var child))
                    {
                        child = new SchemaNode(mountingPointName);
                        current._children[mountingPointName] = child;
                    }

                    if (i == mountingPoints.Length - 1)
                    {
                        var schema = new JsonSchemaBuilder()
                            .Ref($"#/$defs/{componentDefinition.ComponentName}");

                        current._children[mountingPointName]._schemas.Add(schema);
                    }

                    current = child;
                }
            }
        }

        return root;
    }
}