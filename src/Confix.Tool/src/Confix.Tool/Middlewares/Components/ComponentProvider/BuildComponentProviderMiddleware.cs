using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Entities.Components;

namespace Confix.Tool.Middlewares;

public sealed class BuildComponentProviderMiddleware : IMiddleware
{
    private readonly IComponentProviderFactory _factory;

    public BuildComponentProviderMiddleware(IComponentProviderFactory factory)
    {
        _factory = factory;
    }

    /// <inheritdoc />
    public Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        var configuration = context.Features.Get<ConfigurationFeature>();

        var definitions = configuration.Project?.ComponentProviders ??
            Array.Empty<ComponentProviderDefinition>();

        var executor = ComponentProviderExecutor.FromDefinitions(_factory, definitions);

        context.Features.Set(new ComponentProviderExecutorFeature(executor));

        context.Logger.LoadedComponentProvider();

        return next(context);
    }
}

file static class Log
{
    public static void LoadedComponentProvider(this IConsoleLogger console)
    {
        console.Success("Component inputs loaded");
    }
}
