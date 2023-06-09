using System.CommandLine;
using System.ComponentModel;
using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Entities.Components;
using Confix.Tool.Entities.Components.DotNet;
using Confix.Tool.Middlewares;
using Confix.Tool.Middlewares.JsonSchemas;

namespace Confix.Tool.Commands.Project;

public sealed class ProjectReloadCommand : Command
{
    public ProjectReloadCommand() : base("reload")
    {
        this
            .AddPipeline()
            .Use<LoadConfigurationMiddleware>()
            .Use<JsonSchemaCollectionMiddleware>()
            .Use<ConfigurationAdapterMiddleware>()
            .Use<BuildComponentProviderMiddleware>()
            .UseHandler<IProjectComposer, ISchemaStore>(InvokeAsync);

        Description = "Reloads the schema of a project";
    }

    private static async Task InvokeAsync(
        IMiddlewareContext context,
        IProjectComposer projectComposer,
        ISchemaStore schemaStore)
    {
        context.SetStatus("Reloading the schema of the project...");

        var cancellationToken = context.CancellationToken;

        var jsonSchemas = context.Features.Get<JsonSchemaFeature>();
        var configuration = context.Features.Get<ConfigurationFeature>();
        configuration.EnsureProjectScope();

        var project = configuration.EnsureProject();
        var repository = configuration.EnsureRepository();

        var componentProvider =
            context.Features.Get<ComponentProviderExecutorFeature>().Executor;

        var providerContext =
            new ComponentProviderContext(context.Logger, cancellationToken, project, repository);

        context.SetStatus("Loading components...");
        await componentProvider.ExecuteAsync(providerContext);
        var components = providerContext.Components;
        context.Logger.LogComponentsLoaded(components);

        context.SetStatus("Composing the schema...");
        var jsonSchema = projectComposer.Compose(components);
        context.Logger.LogSchemaCompositionCompleted(project);

        var schemaFile = await schemaStore
            .StoreAsync(repository, project, jsonSchema, cancellationToken);

        var jsonSchemaDefinition = new JsonSchemaDefinition()
        {
            Project = project,
            Repository = repository.Directory!,
            //TODO Here we have to get it from the configuration files
            FileMatch = new List<string>() { "**/appsettings*.json" },
            SchemaFile = schemaFile,
            RelativePathToProject =
                Path.GetRelativePath(repository.Directory!.FullName, project.Directory!.FullName)
        };
        jsonSchemas.Schemas.Add(jsonSchemaDefinition);
    }
}

file static class Extensions
{
    public static void EnsureProjectScope(this ConfigurationFeature configuration)
    {
        if (configuration.Scope is not ConfigurationScope.Project)
        {
            App.Log.ScopeHasToBeAProject();
            throw new ExitException();
        }
    }

    public static ProjectDefinition EnsureProject(this ConfigurationFeature configuration)
    {
        if (configuration.Project is not { } project)
        {
            App.Log.NoProjectWasFound();
            throw new ExitException();
        }

        return project;
    }

    public static RepositoryDefinition EnsureRepository(this ConfigurationFeature configuration)
    {
        if (configuration.Repository is not { } repository)
        {
            App.Log.NoRepositoryWasFound();
            throw new ExitException();
        }

        return repository;
    }
}

file static class Log
{
    public static void ScopeHasToBeAProject(this IConsoleLogger console)
    {
        console.Error("Scope has to be a project");
    }

    public static void NoProjectWasFound(this IConsoleLogger console)
    {
        console.Error("No project was found");
    }

    public static void NoRepositoryWasFound(this IConsoleLogger console)
    {
        console.Error("No repository was found");
    }

    public static void LogComponentsLoaded(
        this IConsoleLogger console,
        ICollection<Abstractions.Component> components)
    {
        console.Success($"Loaded {components.Count} components");
        foreach (var component in components)
        {
            console.Information($"-  @{component.Provider}/{component.ComponentName}");
        }
    }

    public static void LogSchemaCompositionCompleted(
        this IConsoleLogger console,
        ProjectDefinition project)
    {
        console.Success($"Schema composition completed for project {project.Name}");
    }
}
