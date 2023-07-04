using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares.Encryption;

namespace Confix.Tool.Middlewares;

public static class ConfigurationFilesCommandPipelineBuilderExtensions
{
    public static IPipelineDescriptor UseWriteConfigurationFiles(this IPipelineDescriptor builder)
    {
        builder.AddOption(EncryptionOption.Instance);
        builder.Use<OptionalEncryptionMiddleware>();
        builder.Use<WriteConfigurationFileMiddleware>();

        return builder;
    }

    public static IPipelineDescriptor UseReadConfigurationFiles(this IPipelineDescriptor builder)
    {
        builder.AddOption(OutputFileOption.Instance);
        builder.Use<ReadConfigurationFileMiddleware>();

        return builder;
    }

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
        logger.Debug("Skipped command execution build because no configuration files were found");
    }
}
