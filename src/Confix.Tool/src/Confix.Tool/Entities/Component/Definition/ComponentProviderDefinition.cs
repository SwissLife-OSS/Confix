using System.Text.Json.Nodes;

namespace Confix.Tool.Abstractions;

public sealed class ComponentProviderDefinition
{
    public string Name { get; }

    public string Type { get; }

    public JsonObject Value { get; }

    public ComponentProviderDefinition(string name, string type, JsonObject values)
    {
        Name = name;
        Type = type;
        Value = values;
    }

    public static ComponentProviderDefinition From(ComponentProviderConfiguration configuration)
    {
        var name = configuration.Name ??
            throw new InvalidOperationException("Name is not defined.");
        var type = configuration.Type ??
            throw new InvalidOperationException("Type is not defined.");
        var values = configuration.Values;

        return new ComponentProviderDefinition(name, type, values);
    }
}
