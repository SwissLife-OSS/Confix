using System.Text.Json.Nodes;

namespace Confix.Tool.Schema;

public sealed class LeafJsonDocumentVisitor
    : JsonDocumentVisitor<LeafJsonDocumentVisitor.Context>
{
    private LeafJsonDocumentVisitor()
    {
    }

    /// <inheritdoc />
    protected override void Visit(JsonValue value, Context context)
    {
        context.Visit(value);
    }

    public static void Visit(JsonNode node, Action<JsonValue> visit)
    {
        var context = new Context(visit);
        Instance.Visit(node, context);
    }

    private static LeafJsonDocumentVisitor Instance { get; } = new();

    public sealed record Context(Action<JsonValue> Visit);
}
