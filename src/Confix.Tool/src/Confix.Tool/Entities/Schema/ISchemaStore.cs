using Confix.Tool.Abstractions;
using Json.Schema;

namespace Confix.Tool.Entities.Components.DotNet;

public interface ISchemaStore
{
    Task<FileInfo> StoreAsync(
        RepositoryDefinition repository,
        ProjectDefinition project,
        JsonSchema schema,
        CancellationToken cancellationToken);
}
