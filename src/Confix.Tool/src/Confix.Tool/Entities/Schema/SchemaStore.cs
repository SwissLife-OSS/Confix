using System.Text.Json;
using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Schema;
using Json.Schema;

namespace Confix.Tool.Entities.Components.DotNet;

public sealed class SchemaStore : ISchemaStore
{
    /// <inheritdoc />
    public async Task<FileInfo> StoreAsync(
        RepositoryDefinition repository,
        ProjectDefinition project,
        JsonSchema schema,
        CancellationToken cancellationToken)
    {
        var schemaFile = GetSchemaFile(repository, project);

        await using var stream = schemaFile.OpenReplacementStream();

        await JsonSerializer
            .SerializeAsync(stream, schema, cancellationToken: cancellationToken);

        App.Log.SchemaIsStored(schemaFile);

        return schemaFile;
    }

    private static FileInfo GetSchemaFile(
        RepositoryDefinition repository,
        ProjectDefinition project)
    {
        if (repository.Directory is not { } directory)
        {
            throw new ExitException("Could not find directory of repository");
        }

        // the name of the schema is the name of the project with the .schema.json extension 
        // this way we guarantee that the schema name is unique
        var schemaName = project.Name + FileNames.Extensions.SchemaJson;

        // the schemas folder is in the root of the repository under .confix/schemas
        var schemasFolder = directory.GetSchemasFolder().EnsureFolder().FullName;

        return new FileInfo(Path.Combine(schemasFolder, schemaName));
    }
}

file static class Log
{
    public static void SchemaIsStored(this IConsoleLogger console, FileInfo schemaFile)
    {
        console.Success($"Schema is stored at '{schemaFile.FullName}'");
    }
}
