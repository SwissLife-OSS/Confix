using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Commands.Solution;
using Confix.Tool.Commands.Temp;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Entities.Components;
using Confix.Tool.Middlewares;
using Confix.Tool.Schema;
using static System.StringSplitOptions;

namespace Confix.Tool.Commands.Component;

public sealed class AddComponentPipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder
            .AddArgument(ComponentNameArgument.Instance)
            .AddOption(VersionOption.Instance)
            .Use<LoadConfigurationMiddleware>()
            .UseReadConfigurationFiles()
            .Use<BuildComponentProviderMiddleware>()
            .UseHandler(InvokeAsync);
    }

    private static async Task InvokeAsync(IMiddlewareContext context)
    {
        context.SetStatus("Searching components...");

        var ct = context.CancellationToken;
        var logger = context.Logger;

        var configuration = context.Features.Get<ConfigurationFeature>();

        configuration.EnsureProjectScope();

        var solution = configuration.EnsureSolution();
        var project = configuration.EnsureProject();

        var componentName = context.Parameter.Get(ComponentNameArgument.Instance).UnwrapParameter();
        var version = context.Parameter.TryGet(VersionOption.Instance, out string value)
            ? value
            : "latest";

        if (await ValidateComponentAsync(context, project, solution, componentName, version, ct))
        {
            await WriteProjectFile(project, ct, componentName, version);
            logger.ComponentWasAdded(componentName);
        }
    }

    private static async Task WriteProjectFile(
        ProjectDefinition project,
        CancellationToken ct,
        string componentName,
        string version)
    {
        var projectJson = project.Directory!.AppendFile(FileNames.ConfixProject);
        var projectJsonNode = JsonNode.Parse(await File.ReadAllTextAsync(projectJson.FullName, ct));
        var componentsNode = projectJsonNode!["components"];
        if (componentsNode is null)
        {
            componentsNode = projectJsonNode["components"] = new JsonObject();
        }

        componentsNode[componentName] = JsonValue.Create(version);
        var options = new JsonSerializerOptions() { WriteIndented = true };
        await File.WriteAllTextAsync(
            projectJson.FullName,
            projectJsonNode.ToJsonString(options),
            ct);
    }

    private static async Task<bool> ValidateComponentAsync(
        IMiddlewareContext context,
        ProjectDefinition project,
        SolutionDefinition solution,
        string componentName,
        string version,
        CancellationToken ct)
    {
        if (componentName.Split("/", TrimEntries | RemoveEmptyEntries) is not
            [['@', .. var provider], var name])
        {
            throw new ExitException($"Invalid component name: {componentName}");
        }

        if (project.Components.Any(component => component.ComponentName == componentName))
        {
            context.Logger.ComponentAlreadyExists(componentName);
            return false;
        }

        var reference =
            new ComponentReferenceDefinition(provider, name, version, true, Array.Empty<string>());
        var providerContext =
            new ComponentProviderContext(context.Logger,
                ct,
                project,
                solution,
                context.Parameter,
                new[] { reference });

        var executor = context.Features.Get<ComponentProviderExecutorFeature>().Executor;
        await executor.ExecuteAsync(providerContext);
        if (providerContext.Components.Count == 0)
        {
            throw new ExitException($"Component '{componentName}' not found");
        }

        return true;
    }
}

public static class Logs
{
    public static void ComponentAlreadyExists(this IConsoleLogger logger, string componentName)
    {
        logger.Success($"Component '{componentName}' already exists");
    }

    public static void ComponentWasAdded(this IConsoleLogger logger, string componentName)
    {
        logger.Success($"Component '{componentName}' was added");
    }
}

public static class Extensions
{
    public static string UnwrapParameter(this string str)
    {
        if (str is ['\"', '\\', .., '\"'])
        {
            return str[2..^1];
        }

        return str;
    }
}
