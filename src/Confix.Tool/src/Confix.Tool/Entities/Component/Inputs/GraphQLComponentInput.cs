using System.Text.Json;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Schema;

namespace Confix.Tool.Entities.Component;

public sealed class GraphQLComponentInput : IComponentInput
{
    public static string Type => "graphql";

    private const string _schemaGraphQL = "schema.graphql";

    /// <inheritdoc />
    public async Task ExecuteAsync(IMiddlewareContext context)
    {
        var cancellationToken = context.CancellationToken;

        var configuration = context.Features.Get<ConfigurationFeature>();

        if (configuration.Scope is not ConfigurationScope.Component ||
            configuration.ConfigurationFiles.Component is null)
        {
            throw new InvalidOperationException(
                "Component input has to be executed in a component directory");
        }

        var configurationFile = configuration.ConfigurationFiles.Component.SourceFiles
            .First(x => x.Name == FileNames.ConfixComponent);

        var schemaGraphQlFile =
            new FileInfo(Path.Combine(configurationFile.DirectoryName!, _schemaGraphQL));

        context.Logger.SearchingForSchemaGraphQl(schemaGraphQlFile);

        if (!schemaGraphQlFile.Exists)
        {
            context.Logger.SchemaGraphQLNotFound();
            return;
        }

        context.Logger.SchemaGraphQLFound(schemaGraphQlFile);

        var schemaJsonFile =
            new FileInfo(Path.Combine(configurationFile.DirectoryName!, FileNames.Schema));

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
    public static void SchemaGraphQLFound(
        this IConsoleLogger console,
        FileSystemInfo schemaFile)
    {
        console.Information("GraphQL Schema was found: [dim]{0}[/]", schemaFile.FullName);
    }

    public static void SchemaGraphQLNotFound(this IConsoleLogger console)
    {
        console.Information("GraphQL Schema was not found. Skipping GraphQL provider");
    }

    public static void SearchingForSchemaGraphQl(
        this IConsoleLogger console,
        FileSystemInfo schemaFile)
    {
        console.Debug("Searching in {0} for GraphQL Schema", schemaFile.FullName);
    }

    public static void GeneratedSchemaBasedOnGraphQL(
        this IConsoleLogger console,
        FileSystemInfo schemaFile)
    {
        console.Information("Generated schema based on GraphQL Schema: [dim]{0}[/]",
            schemaFile.FullName);
    }
}
