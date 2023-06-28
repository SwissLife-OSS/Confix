using System.Text.Json;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Schema;

namespace Confix.Tool.Entities.Components;

public sealed class GraphQlComponentInput : IComponentInput
{
    public static string Type => "graphql";

    private const string SchemaGraphQl = "schema.graphql";

    /// <inheritdoc />
    public async Task ExecuteAsync(IMiddlewareContext context)
    {
        var cancellationToken = context.CancellationToken;

        var configuration = context.Features.Get<ConfigurationFeature>();

        if (configuration.Scope is not ConfigurationScope.Component ||
            configuration.ConfigurationFiles.Component is null)
        {
            throw new ExitException("Component input has to be executed in a component directory");
        }

        var configurationFile = configuration.ConfigurationFiles.Component.SourceFiles
            .First(x => x.File.Name == FileNames.ConfixComponent);

        var schemaGraphQlFile =
            new FileInfo(Path.Combine(configurationFile.File.DirectoryName!, SchemaGraphQl));

        context.Logger.SearchingForSchemaGraphQl(schemaGraphQlFile);

        if (!schemaGraphQlFile.Exists)
        {
            context.Logger.SchemaGraphQlNotFound();
            return;
        }

        context.Logger.SchemaGraphQlFound(schemaGraphQlFile);

        var schemaJsonFile =
            new FileInfo(Path.Combine(configurationFile.File.DirectoryName!, FileNames.Schema));

        var schema =
            await SchemaHelpers.LoadSchemaAsync(schemaGraphQlFile.FullName, cancellationToken);

        var jsonSchema = schema.ToJsonSchema().Build();

        if (schemaJsonFile.Exists)
        {
            schemaJsonFile.Delete();
        }

        await using var fileStream = File.OpenWrite(schemaJsonFile.FullName);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        await JsonSerializer.SerializeAsync(fileStream, jsonSchema, options, cancellationToken);

        context.Logger.GeneratedSchemaBasedOnGraphQL(schemaJsonFile);
    }
}

file static class Log
{
    public static void SchemaGraphQlFound(
        this IConsoleLogger console,
        FileSystemInfo schemaFile)
    {
        console.Information("GraphQL Schema was found: {0} [dim]{1}[/]",
            schemaFile.ToLink(),
            schemaFile.FullName);
    }

    public static void SchemaGraphQlNotFound(this IConsoleLogger console)
    {
        console.Information("GraphQL Schema was not found. Skipping GraphQL provider");
    }

    public static void SearchingForSchemaGraphQl(
        this IConsoleLogger console,
        FileSystemInfo schemaFile)
    {
        console.Debug("Searching in {0} for GraphQL Schema", schemaFile.ToLink());
        console.Trace($" -> {schemaFile.FullName}");
    }

    public static void GeneratedSchemaBasedOnGraphQL(
        this IConsoleLogger console,
        FileSystemInfo schemaFile)
    {
        console.Information("Generated schema based on GraphQL Schema:{0} [dim]{1}[/]",
            schemaFile.ToLink(),
            schemaFile.FullName);
    }
}
