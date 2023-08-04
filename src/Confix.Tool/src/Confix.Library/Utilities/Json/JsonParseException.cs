using System.Text.Json.Nodes;

namespace Confix.Utilities.Parsing;

public sealed class JsonParseException : Exception
{
    public JsonParseException(JsonNode node, string message) : base(message)
    {
        Node = node;
    }

    public JsonNode Node { get; set; }
}
