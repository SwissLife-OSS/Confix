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

        var componentProvider =
            context.Features.Get<ComponentProviderExecutorFeature>().Executor;

        var providerContext =
            new ComponentProviderContext(context.Logger, cancellationToken, project, solution);

        context.SetStatus("Loading components...");
        await componentProvider.ExecuteAsync(providerContext);

        context.SetOutput(providerContext.Components);
        
        if (providerContext.Components.Count == 0)
        {
            context.Logger.Information("No components found");
            return;
        }

        foreach (var file in providerContext.Components)
        {
            context.Logger.Information(" - " + file.ComponentName);
        }
    }
}
