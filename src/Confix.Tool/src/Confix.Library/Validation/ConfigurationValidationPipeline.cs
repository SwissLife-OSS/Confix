using System.CommandLine;
using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Middlewares.Encryption;
using Confix.Tool.Middlewares.Project;

namespace Confix.Tool.Validation;

public static class ConfigurationValidationPipeline
{
    public static readonly Option<bool> ExportSchemaOption = new("--export-schema")
    {
        Description = "Export an IDE schema for a code-first project."
    };

    public static async Task DispatchAsync(IMiddlewareContext context, MiddlewareDelegate next, bool write)
    {
        // The provider is selected per environment, so the environment is resolved before dispatch.
        // The legacy pipeline resolves it again downstream, which is idempotent.
        await new Pipeline(builder => builder.UseEnvironment()).ExecuteAsync(context);
        var project = context.Features.Get<ConfigurationFeature>().EnsureProject();
        var environment = context.Features.Get<EnvironmentFeature>().ActiveEnvironment;
        var settings = project.Validation?.Merge(environment.Validation) ?? environment.Validation;

        if (settings is null || settings.EffectiveType == "json-schema")
        {
            await next(context);
            return;
        }

        IConfigurationValidator validator = settings.EffectiveType switch
        {
            "dotnet-options" => new DotnetOptionsValidator(),
            _ => throw new ExitException("Unsupported configuration validation provider: " + settings.EffectiveType)
        };
        var exportSchema = write && ((environment.ExportSchema ?? project.ExportSchema ?? false) ||
            (context.Parameter.TryGet(ExportSchemaOption, out bool requested) && requested));

        context.Features.Set(new ConfigurationReadOnlyFeature(!write));
        await new Pipeline(builder => builder
            .UseReadConfigurationFiles()
            .Use(async (current, continuePipeline) =>
            {
                await ConfigurationInput.ApplyAsync(current, settings);
                await continuePipeline(current);
            })
            .Use<VariableMiddleware>()
            .Use<BuildProjectMiddleware>()
            .Use((current, _) => ValidateAsync(current, validator, settings, write, exportSchema)))
            .ExecuteAsync(context);
    }

    private static async Task ValidateAsync(
        IMiddlewareContext context,
        IConfigurationValidator validator,
        ValidationConfiguration settings,
        bool write,
        bool exportSchema)
    {
        if (context.Features.Get<ConfigurationFileFeature>().Files.Count == 0)
        {
            throw new ExitException("Configuration validation requires input.");
        }

        var schema = await validator.ValidateAsync(context, settings, exportSchema);
        if (write)
        {
            await new Pipeline(builder => builder
                .Use<OptionalEncryptionMiddleware>()
                .Use((current, _) => ConfigurationOutput.PublishAsync(current)))
                .ExecuteAsync(context);

            if (exportSchema && schema is not null)
            {
                var directory = context.Features.Get<ConfigurationFeature>().EnsureProject().Directory!.FullName;
                await ConfigurationOutput.ExportSchemaAsync(directory, schema, context.CancellationToken);
                context.Logger.Information("Exported confix.ide.schema.json.");
            }
        }

        context.Logger.Information("Configuration validation succeeded.");
    }
}
