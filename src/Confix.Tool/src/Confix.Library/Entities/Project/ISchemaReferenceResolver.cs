using Json.Schema;

namespace Confix.Tool.Entities.Components.DotNet;

public interface ISchemaReferenceResolver
{
    JsonSchema? Resolve(Uri uri);
}
