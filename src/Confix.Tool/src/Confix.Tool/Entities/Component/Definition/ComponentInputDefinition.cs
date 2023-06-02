using System.Text.Json.Nodes;

namespace Confix.Tool.Abstractions;

public sealed class ComponentInputDefinition
{
    public ComponentInputDefinition(string type, JsonNode? value)
    {
        Type = type;
        Value = value;
    }

    public string Type { get; }

    public JsonNode? Value { get; }
}
