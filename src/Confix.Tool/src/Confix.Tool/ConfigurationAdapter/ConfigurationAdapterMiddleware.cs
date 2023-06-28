using System.Diagnostics.CodeAnalysis;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares.JsonSchemas;
using Confix.Tool.Schema;

namespace Confix.Tool.Middlewares;

public sealed class ConfigurationAdapterMiddleware : IMiddleware
{
    private readonly IEnumerable<IConfigurationAdapter> _configurationAdapters;

    public ConfigurationAdapterMiddleware(IEnumerable<IConfigurationAdapter> configurationAdapters)
    {
        _configurationAdapters = configurationAdapters;
    }

    /// <inheritdoc />
    public async Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        var configuration = context.Features.Get<ConfigurationFeature>();
        if (!configuration.TryGetSolution(out var solutionFile))
        {
            context.Logger.NoSolutionFileFound();
            throw new ExitException();
        }

        await next(context);

        var jsonSchemaFeature = context.Features.Get<JsonSchemaFeature>();
        var adapterContext = new ConfigurationAdapterContext
        {
            Logger = context.Logger,
            CancellationToken = context.CancellationToken,
            Schemas = jsonSchemaFeature.Schemas.ToArray(),
            SolutionRoot = solutionFile.GetDirectory()
        };

        context.SetStatus("Updating the schemas configuration for IDE...");
        foreach (var adapter in _configurationAdapters)
        {
            await adapter.UpdateJsonSchemasAsync(adapterContext);
        }
    }
}

file static class Logs
{
    public static void NoSolutionFileFound(this IConsoleLogger logger)
    {
        logger.Error(
            "No solution file found, could not load VSCode Settings. Please make sure that the current directory is a Confix solution.");
    }
}

file static class Extensions
{
    public static bool TryGetSolution(
        this ConfigurationFeature feature,
        [NotNullWhen(true)] out FileInfo? solutionFile)
    {
        var solution =
            feature.ConfigurationFiles.FirstOrDefault(x => x.File.Name == FileNames.ConfixSolution);
        solutionFile = solution?.File;

        return solution is not null;
    }
}
