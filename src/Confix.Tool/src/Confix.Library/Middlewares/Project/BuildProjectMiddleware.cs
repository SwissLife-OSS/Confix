using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Variables;

namespace Confix.Tool.Middlewares.Project;

public sealed class BuildProjectMiddleware : IMiddleware
{
    /// <inheritdoc />
    public async Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        context.SetStatus("Building the project files");

        var cancellationToken = context.CancellationToken;

        var files = context.Features.Get<ConfigurationFileFeature>().Files;
        var configuration = context.Features.Get<ConfigurationFeature>();
        configuration.EnsureProjectScope();

        var variableReplacer =
            context.Features.Get<VariablesFeature>().Replacer;

        foreach (var file in files)
        {
            var content = await file.TryLoadContentAsync(cancellationToken);

            if (content is null)
            {
                continue;
            }

            context.Logger.ReplaceVariablesOfConfigurationFile(file);

            file.Content = await variableReplacer
                .RewriteOrThrowAsync(content, context.CancellationToken);
        }

        await next(context);
    }
}

file static class Logs
{
    public static void ReplaceVariablesOfConfigurationFile(
        this IConsoleLogger logger,
        ConfigurationFile file)
    {
        logger.Debug($"Replacing variables in configuration file '{file.InputFile.Name}'");
    }
}
