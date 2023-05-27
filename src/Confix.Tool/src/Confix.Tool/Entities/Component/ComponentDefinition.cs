using Json.Schema;

namespace Confix.Tool.Abstractions;

public class ComponentDefinition
{
    public ComponentDefinition(string name, JsonSchema schema)
    {
        Name = name;
        Schema = schema;
    }

    public string Name { get; }

    public JsonSchema Schema { get; }
}