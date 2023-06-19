using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Commands.Temp;
using Confix.Tool.Common.Pipelines;
using Microsoft.Extensions.Logging;

namespace Confix.Tool.Middlewares;

public sealed class ReadConfigurationFileMiddleware : IMiddleware
{
    private readonly IConfigurationFileProviderFactory _factory;

    public ReadConfigurationFileMiddleware(IConfigurationFileProviderFactory factory)
    {
        _factory = factory;
    }

    /// <inheritdoc />
    public async Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        context.SetStatus("Loading configuration files...");

        var configuration = context.Features.Get<ConfigurationFeature>();
        var project = configuration.EnsureProject();

        var feature = new ConfigurationFileFeature();

        foreach (var file in project.ConfigurationFiles)
        {
            var provider = _factory.Create(file);

            var factoryContext = new ConfigurationFileContext
            {
                Logger = context.Logger,
                Definition = file,
                Project = project
            };

            foreach (var configurationFile in provider.GetConfigurationFiles(factoryContext))
            {
                feature.Files.Add(configurationFile);
            }
        }

        ApplyOutputFileOption(context, project, feature);

        context.Features.Set(feature);

        await next(context);
    }

    private static void ApplyOutputFileOption(
        IMiddlewareContext context,
        ProjectDefinition project,
        ConfigurationFileFeature feature)
    {
        if (!context.Parameter.TryGet(OutputFileOption.Instance, out FileInfo outputFileOption))
        {
            return;
        }

        context.Logger.OutputFileOverride(outputFileOption.FullName);
        
        foreach (var t in feature.Files)
        {
            t.OutputFile = outputFileOption;
        }
    }
}

file static class Log
{
    public static void OutputFileOverride(this IConsoleLogger logger, string outputFile)
    {
        logger.Debug($"Overriding the output file with '{outputFile}'");
    }
}
