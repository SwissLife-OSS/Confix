using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Schema;
using Json.Schema;

namespace Confix.Tool.Entities.Components.DotNet;

public sealed class SchemaStore : ISchemaStore
{
    /// <inheritdoc />
    public async Task<FileInfo> StoreAsync(
        SolutionDefinition solution,
        ProjectDefinition project,
        JsonSchema schema,
        CancellationToken cancellationToken)
    {
        var schemaFile = GetSchemaFile(solution, project);

        await using var stream = schemaFile.OpenReplacementStream();

        await JsonSerializer.SerializeAsync(
            stream,
            schema,
            new JsonSerializerOptions() { WriteIndented = true },
            cancellationToken);

        App.Log.SchemaIsStored(schemaFile);

        return schemaFile;
    }

    /// <inheritdoc />
    public bool TryLoad(
        SolutionDefinition solution,
        ProjectDefinition project,
        [NotNullWhen(true)] out JsonSchema? schema)
    {
        var schemaFile = GetSchemaFile(solution, project);

        if (!schemaFile.Exists)
        {
            schema = null;
            return false;
        }

        schema = JsonSchema.FromFile(schemaFile.FullName);
        return true;
    }

    private static FileInfo GetSchemaFile(
        SolutionDefinition solution,
        ProjectDefinition project)
    {
        if (solution.Directory is not { } directory)
        {
            throw new ExitException("Could not find directory of solution");
        }

        // the name of the schema is the name of the project with the .schema.json extension 
        // this way we guarantee that the schema name is unique
        var schemaName = project.Name + FileNames.Extensions.SchemaJson;

        // the schemas folder is in the root of the solution under .confix/schemas
        var schemasFolder = directory.GetSchemasFolder().EnsureFolder().FullName;

        return new FileInfo(Path.Combine(schemasFolder, schemaName));
    }
}

file static class Log
{
    public static void SchemaIsStored(this IConsoleLogger console, FileInfo schemaFile)
    {
        console.Success($"Schema is stored at {schemaFile.ToLink()}");
        console.Debug($" -> {schemaFile.FullName}");
    }
}
