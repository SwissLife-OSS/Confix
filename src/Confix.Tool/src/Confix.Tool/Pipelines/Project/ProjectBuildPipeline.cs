using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Middlewares.Project;

namespace Confix.Tool.Commands.Project;

public sealed class ProjectBuildPipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder
            .Use<LoadConfigurationMiddleware>()
            .UseReadConfigurationFiles()
            .UseEnvironment()
            .Use<BuildComponentsOfProjectMiddleware>()
            .UseCompleteWhenNoConfigurationFiles()
            .Use<VariableMiddleware>()
            .Use<BuildProjectMiddleware>()
            .Use<ValidationMiddleware>()
            .UseWriteConfigurationFiles();
    }
}

file static class Extensions
{
    public static IPipelineDescriptor UseCompleteWhenNoConfigurationFiles(
        this IPipelineDescriptor builder)
    {
        builder.Use(Middleware);

        return builder;
    }

    private static Task Middleware(IMiddlewareContext context, MiddlewareDelegate next)
    {
        var files = context.Features.Get<ConfigurationFileFeature>().Files;

        if (files.Count == 0)
        {
            context.Logger.SkippedProjectBuildBecauseNoConfigurationFilesWereFound();
            return Task.CompletedTask;
        }

        return next(context);
    }
}

file static class Logs
{
    public static void SkippedProjectBuildBecauseNoConfigurationFilesWereFound(
        this IConsoleLogger logger)
    {
        logger.Debug("Skipped project build because no configuration files were found");
    }
}
