using System.Text.Json;

namespace Confix.Tool.Abstractions;

public sealed class ComponentDefinition
{
    public const string DefaultName = "__Default";

    public ComponentDefinition(
        string name,
        IReadOnlyList<ComponentInputDefinition> inputs,
        IReadOnlyList<ComponentOutputDefinition> outputs)
    {
        Name = name;
        Inputs = inputs;
        Outputs = outputs;
    }

    public string Name { get; }

    public IReadOnlyList<ComponentInputDefinition> Inputs { get; }

    public IReadOnlyList<ComponentOutputDefinition> Outputs { get; }

    public void WriteTo(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();

        writer.WriteString(ComponentConfiguration.FieldNames.Name, Name);

        writer.WritePropertyName(ComponentConfiguration.FieldNames.Inputs);
        writer.WriteStartArray();
        foreach (var input in Inputs)
        {
            input.WriteTo(writer);
        }

        writer.WriteEndArray();

        writer.WritePropertyName(ComponentConfiguration.FieldNames.Outputs);
        writer.WriteStartArray();
        foreach (var output in Outputs)
        {
            output.WriteTo(writer);
        }

        writer.WriteEndArray();

        writer.WriteEndObject();
    }

    public static ComponentDefinition From(ComponentConfiguration configuration)
    {
        var name = string.IsNullOrWhiteSpace(configuration.Name) ? DefaultName : configuration.Name;

        var inputs = configuration.Inputs
            ?.Select(x => new ComponentInputDefinition(x.Type!, x.Value))
            .ToArray() ?? Array.Empty<ComponentInputDefinition>();

        var outputs = configuration.Outputs
                ?.Select(x => new ComponentOutputDefinition(x.Type!, x.Value))
                .ToArray()
            ?? Array.Empty<ComponentOutputDefinition>();

        return new ComponentDefinition(name, inputs, outputs);
    }
}
