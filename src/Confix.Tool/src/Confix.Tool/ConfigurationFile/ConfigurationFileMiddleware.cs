using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Schema;
using Confix.Utilities.Json;
using Spectre.Console;

namespace Confix.Tool.Middlewares;

public sealed class ConfigurationFileMiddleware : IMiddleware
{
    private readonly IConfigurationFileProviderFactory _factory;

    public ConfigurationFileMiddleware(IConfigurationFileProviderFactory factory)
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

        context.Features.Set(feature);

        await next(context);

        context.SetStatus("Persisting configuration changes");
        foreach (var file in feature.Files)
        {
            if (!file.HasContentChanged)
            {
                continue;
            }

            context.Logger.PersistingConfigurationFile(file);

            await using var stream = file.File.OpenReplacementStream();
            await file.Content.SerializeToStreamAsync(stream, context.CancellationToken);
        }
    }
}

file static class Logs
{
    public static void PersistingConfigurationFile(
        this IConsoleLogger console,
        ConfigurationFile file)
    {
        console.Information($"Persisting configuration file '{file.File.FullName}'");
    }
}
