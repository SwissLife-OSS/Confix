using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Entities.Components;
using Confix.Tool.Middlewares;
using Confix.Tool.Middlewares.JsonSchemas;
using Confix.Tool.Middlewares.Project;

namespace Confix.Tool.Commands.Component;

public sealed class ListComponentPipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder
            .AddOption(FormatOption.Instance)
            .Use<LoadConfigurationMiddleware>()
            .UseReadConfigurationFiles()
            .UseEnvironment()
            .UseBuildComponentsOfProject()
            .Use<JsonSchemaCollectionMiddleware>()
            .Use<ConfigurationAdapterMiddleware>()
            .Use<BuildComponentProviderMiddleware>()
            .UseHandler(InvokeAsync);
    }

    private static async Task InvokeAsync(IMiddlewareContext context)
    {
        context.SetStatus("Listing components...");

        var cancellationToken = context.CancellationToken;

        var configuration = context.Features.Get<ConfigurationFeature>();

        configuration.EnsureProjectScope();

        var project = configuration.EnsureProject();
        var solution = configuration.EnsureSolution();

        context.SetStatus("Loading components...");
        
        var components = await context.Features.Get<ComponentProviderExecutorFeature>()
            .Executor.LoadComponents(solution, project, cancellationToken);

        context.SetOutput(components);

        if (components.Count == 0)
        {
            context.Logger.Information("No components found");
        }
    }
}