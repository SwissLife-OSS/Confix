using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Commands.Project;
using Confix.Tool.Commands.Solution;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Entities.Components.DotNet;
using Confix.Tool.Schema;
using Confix.Utilities.Json;
using Json.Schema;

namespace Confix.Tool.Middlewares.Project;

public sealed class InitializeConfigurationDefaultValues : IMiddleware
{
    private readonly ISchemaStore _schemaStore;

    public InitializeConfigurationDefaultValues(ISchemaStore schemaStore)
    {
        _schemaStore = schemaStore;
    }

    /// <inheritdoc />
    public async Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        context.SetStatus("Searching for the schema of the project...");

        var cancellationToken = context.CancellationToken;

        var configuration = context.Features.Get<ConfigurationFeature>();

        configuration.EnsureProjectScope();

        var project = configuration.EnsureProject();
        var solution = configuration.EnsureSolution();

        var files = context.Features.Get<ConfigurationFileFeature>().Files;

        var jsonSchema = await GetJsonSchema(context, solution, project);

        foreach (var file in files)
        {
            var content = await file.TryLoadContentAsync(cancellationToken);
            if (content is null)
            {
                continue;
            }

            content = DefaultValueVisitor.ApplyDefaults(jsonSchema, content);

            context.Logger.PersistingConfigurationFile(file);

            // we ensure to replace the input file as we want to add the properties
            await using var stream = file.InputFile.OpenReplacementStream();
            await content.SerializeToStreamAsync(stream, context.CancellationToken);
            
            // we update the content of the file so later middlewares can use it
            file.Content = content;
        }

        await next(context);
    }

    private async Task<JsonSchema> GetJsonSchema(
        IMiddlewareContext context,
        SolutionDefinition solution,
        ProjectDefinition project)
    {
        if (!_schemaStore.TryLoad(solution, project, out var schema))
        {
            context.Logger.SchemaNotFoundInitiateSchemaReload(project);
            var projectRestorePipeline = new ProjectRestorePipeline();
            await projectRestorePipeline.ExecuteAsync(context);

            if (!_schemaStore.TryLoad(solution, project, out schema))
            {
                throw new ExitException("The schema could not be loaded.");
            }
        }
        else
        {
            context.Logger.LoadSchemaFromCache(project);
        }

        return schema;
    }
}

file static class Log
{
    public static void SchemaNotFoundInitiateSchemaReload(
        this IConsoleLogger console,
        ProjectDefinition project)
    {
        console.Warning(
            $"The schema of the project was not found. Initiating schema reload for project:{project.Name.ToLink(project.Directory!)}");
    }

    public static void LoadSchemaFromCache(
        this IConsoleLogger console,
        ProjectDefinition project)
    {
        console.Inform(
            $"Loaded schema from [bold]cache[/] for project {project.Name.ToLink(project.Directory!)}");
    }

    public static void PersistingConfigurationFile(
        this IConsoleLogger console,
        ConfigurationFile file)
    {
        console.Information($"Persisting configuration file {file.OutputFile.ToLink()}");
        console.Debug($" -> {file.OutputFile.FullName}");
    }
}
