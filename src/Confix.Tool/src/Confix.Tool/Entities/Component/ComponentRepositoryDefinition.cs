using System.Text.Json.Nodes;

namespace Confix.Tool.Abstractions;

public sealed class ComponentRepositoryDefinition
{
    public ComponentRepositoryDefinition(string name, string type, JsonObject values)
    {
        Name = name;
        Type = type;
        Values = values;
    }

    public string Name { get; }

    public string Type { get; }

    public JsonObject Values { get; }

    public static ComponentRepositoryDefinition From(ComponentRepositoryConfiguration configuration)
    {
        var name = configuration.Name ??
            throw new InvalidOperationException("Name is not defined.");
        var type = configuration.Type ??
            throw new InvalidOperationException("Type is not defined.");
        var values = configuration.Values;

        return new ComponentRepositoryDefinition(name, type, values);
    }
}
