using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Schema;
using Confix.Utilities.Json;
using Spectre.Console;

namespace Confix.Tool.Middlewares;

public sealed class WriteConfigurationFileMiddleware : IMiddleware
{
    /// <inheritdoc />
    public async Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        await next(context);
        context.SetStatus("Persisting configuration changes");

        var feature = context.Features.Get<ConfigurationFileFeature>();

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
        console.Information($"Persisting configuration file {file.File.ToLink()}");
        console.Debug($" -> {file.File.FullName}");
    }
}
