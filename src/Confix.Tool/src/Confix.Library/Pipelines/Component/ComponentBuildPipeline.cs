using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;

namespace Confix.Tool.Commands.Component;

public sealed class ComponentBuildPipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder
            .Use<LoadConfigurationMiddleware>()
            .Use<BuildComponentInputMiddleware>()
            .UseHandler(InvokeAsync);
    }

    private static async Task InvokeAsync(IMiddlewareContext context)
    {
        context.Logger.BuildingComponent();

        var configuration = context.Features.Get<ConfigurationFeature>();
        if (configuration.Scope is not ConfigurationScope.Component)
        {
            context.Logger.WrongComponentScope(configuration.Scope);
            return;
        }

        var executor = context.Features.Get<ComponentInputExecutorFeature>().Executor;

        await executor.ExecuteAsync(context);

        context.Logger.BuildingComponentCompleted();
    }
}

file static class Log
{
    public static void BuildingComponent(this IConsoleLogger console)
    {
        console.Information("Building component...");
    }

    public static void BuildingComponentCompleted(this IConsoleLogger console)
    {
        console.Information("Building component completed");
    }

    public static void WrongComponentScope(
        this IConsoleLogger console,
        ConfigurationScope scope)
    {
        console.Error(
            "{0} has to be executed in a component directory, but was executed in {1}",
            "confix component build".AsCommand(),
            scope.ToString().AsHighlighted());
    }
}
