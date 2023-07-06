using System.Collections.Immutable;
using Json.Schema;

namespace Confix.Tool.Entities.Components.DotNet;

internal class SchemaReferenceResolver : ISchemaReferenceResolver
{
    private IReadOnlyDictionary<string, JsonSchema> _defs;

    public SchemaReferenceResolver(IReadOnlyDictionary<string, JsonSchema> defs)
    {
        _defs = defs;
    }

    public JsonSchema? Resolve(Uri uri)
    {
        var schemaName = uri.OriginalString.Split('/').Last();

        return _defs.TryGetValue(schemaName, out var schema) ? schema : null;
    }

    public static SchemaReferenceResolver From(JsonSchema schema)
    {
        return new(schema.GetDefs() ?? ImmutableDictionary<string, JsonSchema>.Empty);
    }
}
