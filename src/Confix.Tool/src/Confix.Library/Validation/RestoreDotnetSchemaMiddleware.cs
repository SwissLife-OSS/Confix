using Confix.Tool.Abstractions;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Entities.Components.DotNet;
using Confix.Tool.Middlewares;

namespace Confix.Tool.Validation;

/// <summary>
/// Refreshes the stored schema from the option contracts so defaults initialization
/// always scaffolds against the contracts of the current build.
/// </summary>
internal sealed class RestoreDotnetSchemaMiddleware(ISchemaStore schemaStore) : IMiddleware
{
    public async Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        var settings = DotnetOptionsSchemaComposer.ResolveSettings(context);

        // Defaults initialization restores on its own when no schema is cached.
        var skip = context.Parameter.TryGet(NoRestoreOptions.Instance, out bool noRestore)
            && noRestore;

        if (!skip && settings?.EffectiveType == ValidationConfiguration.DotnetOptions)
        {
            await DotnetOptionsSchemaComposer.ComposeAndStoreAsync(context, settings, schemaStore);
        }

        await next(context);
    }
}
