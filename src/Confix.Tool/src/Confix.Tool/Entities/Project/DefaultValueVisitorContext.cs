using Json.Schema;

namespace Confix.Tool.Entities.Components.DotNet;

public sealed class DefaultValueVisitorContext
{
    public ISchemaReferenceResolver ReferenceResolver { get; private init; }

    public Stack<JsonSchema> Schemas { get; } = new();

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
