using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Entities.Components.DotNet;
using Confix.Tool.Middlewares;
using Confix.Tool.Schema;
using Confix.Utilities.Json;
using Json.Schema;
using Spectre.Console;

namespace Confix.Tool.Validation;

/// <summary>
/// Initializes missing required sections from the option contracts. It runs after variables are
/// substituted, because the application has to compose its host to expose its contracts, but it
/// writes to the files as they are on disk so that resolved values never reach the source.
/// </summary>
internal sealed class ScaffoldDotnetDefaultsMiddleware : IMiddleware
{
    public async Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        var settings = DotnetOptionsSchemaComposer.ResolveSettings(context);
        var skip = context.Parameter.TryGet(NoRestoreOptions.Instance, out bool noRestore)
            && noRestore;

        if (skip || settings?.EffectiveType != ValidationConfiguration.DotnetOptions)
        {
            await next(context);

            return;
        }

        var schema = await TryDeriveAsync(context, settings);

        if (schema is not null)
        {
            await ScaffoldAsync(context, schema);
        }

        await next(context);
    }

    private static async Task<JsonSchema?> TryDeriveAsync(
        IMiddlewareContext context,
        ValidationConfiguration settings)
    {
        try
        {
            return await DotnetOptionsSchemaComposer.DeriveAsync(context, settings);
        }
        catch (ExitException ex)
        {
            // An application that cannot compose its host cannot be asked for its contracts.
            context.Logger.Warning(
                $"Skipping configuration scaffolding: {Markup.Escape(ex.Message)}");

            return null;
        }
    }

    private static async Task ScaffoldAsync(IMiddlewareContext context, JsonSchema schema)
    {
        var files = context.Features.Get<ConfigurationFileFeature>().Files;

        foreach (var file in files)
        {
            JsonNode? source;

            try
            {
                source = JsonNode.Parse(await file.InputFile.ReadAllText(context.CancellationToken));
            }
            catch (System.Text.Json.JsonException)
            {
                continue;
            }

            if (source is null)
            {
                continue;
            }

            var scaffolded = DefaultValueVisitor.ApplyDefaults(schema, source);

            if (!JsonNode.DeepEquals(scaffolded, source))
            {
                context.Logger.Information(
                    $"Initialized missing configuration in {Markup.Escape(file.InputFile.Name)}.");

                await using var stream = file.InputFile.OpenReplacementStream();
                await scaffolded.SerializeToStreamAsync(stream, context.CancellationToken);
            }

            // The composed document is validated and published, so it needs the same defaults.
            if (file.Content is { } rendered)
            {
                file.Content = DefaultValueVisitor.ApplyDefaults(schema, rendered);
            }
        }
    }
}
