using System.Diagnostics.CodeAnalysis;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Entities.Components;
using Confix.Tool.Middlewares.JsonSchemas;
using Confix.Tool.Schema;
using Microsoft.Extensions.DependencyInjection;

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
        if (!configuration.TryGetRepository(out var repositoryFile))
        {
            context.Logger.NoRepositoryFileFound();
            throw new ExitException();
        }

        await next(context);

        var jsonSchemaFeature = context.Features.Get<JsonSchemaFeature>();
        var adapterContext = new ConfigurationAdapterContext
        {
            Logger = context.Logger,
            CancellationToken = context.CancellationToken,
            Schemas = jsonSchemaFeature.Schemas.ToArray(),
            RepositoryRoot = repositoryFile.GetDirectory()
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
    public static void NoRepositoryFileFound(this IConsoleLogger logger)
    {
        logger.Error(
            "No repository file found, could not load VSCode Settings. Please make sure that the current directory is a Confix repository.");
    }
}

file static class Extensions
{
    public static bool TryGetRepository(
        this ConfigurationFeature feature,
        [NotNullWhen(true)] out FileInfo? repositoryFile)
    {
        var repository =
            feature.ConfigurationFiles.FirstOrDefault(x => x.Name == FileNames.ConfixRepository);

        repositoryFile = repository;

        return repository is not null;
    }
}
