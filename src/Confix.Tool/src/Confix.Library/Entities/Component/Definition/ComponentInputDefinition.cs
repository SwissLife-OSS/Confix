using System.Text.Json;
using System.Text.Json.Nodes;

namespace Confix.Tool.Abstractions;

public sealed class ComponentInputDefinition
{
    public ComponentInputDefinition(string type, JsonObject value)
    {
        Type = type;
        Value = value;
    }

    public string Type { get; }

    public JsonObject Value { get; }

    public void WriteTo(Utf8JsonWriter writer) => Value.WriteTo(writer);
}
