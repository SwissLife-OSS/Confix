using Confix.Tool.Schema;
using Json.Schema;

namespace Confix.Tool.Entities.Components.DotNet;

public sealed class DefaultValueVisitorContext
{
    public ISchemaReferenceResolver ReferenceResolver { get; private init; }

    public Stack<JsonSchema> Schemas { get; } = new();

    /// <summary>
    /// Determines whether the schema is already being visited.
    /// </summary>
    /// <remarks>
    /// Sub-schemas are materialized on demand and are therefore not reference equal, so the
    /// comparison is done on the underlying JSON.
    /// </remarks>
    public bool IsVisiting(JsonSchema schema)
    {
        var source = schema.AsNode()?.ToJsonString();

        foreach (var visited in Schemas)
        {
            if (ReferenceEquals(visited, schema) ||
                visited.AsNode()?.ToJsonString() == source)
            {
                return true;
            }
        }

        return false;
    }

    public static DefaultValueVisitorContext From(JsonSchema schema)
    {
        var context = new DefaultValueVisitorContext
        {
            ReferenceResolver = SchemaReferenceResolver.From(schema)
        };

        context.Schemas.Push(schema);

        return context;
    }
}
