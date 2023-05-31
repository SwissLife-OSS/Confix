using System.Text.Json.Nodes;
using Confix.Utilities.Json;

namespace Confix.Tool.Abstractions;

public sealed class ComponentConfiguration
{
    private static class FieldNames
    {
        public const string Name = "name";
        public const string Inputs = "inputs";
        public const string Outputs = "outputs";
    }

    public ComponentConfiguration(
        string? name,
        IReadOnlyList<ComponentInputConfiguration>? inputs,
        IReadOnlyList<ComponentOutputConfiguration>? outputs)
    {
        Name = name;
        Inputs = inputs;
        Outputs = outputs;
    }

    public string? Name { get; }

    public IReadOnlyList<ComponentInputConfiguration>? Inputs { get; }

    public IReadOnlyList<ComponentOutputConfiguration>? Outputs { get; }

    public static ComponentConfiguration Parse(JsonNode node)
    {
        var obj = node.ExpectObject();

        var name = obj.MaybeProperty(FieldNames.Name)?.ExpectValue<string>();

        var inputs = obj
            .MaybeProperty(FieldNames.Inputs)
            ?.ExpectArray()
            .WhereNotNull()
            .Select(ComponentInputConfiguration.Parse)
            .ToArray();

        var outputs = obj
            .MaybeProperty(FieldNames.Outputs)
            ?.ExpectArray()
            .WhereNotNull()
            .Select(ComponentOutputConfiguration.Parse)
            .ToArray();

        return new ComponentConfiguration(name, inputs, outputs);
    }
}
