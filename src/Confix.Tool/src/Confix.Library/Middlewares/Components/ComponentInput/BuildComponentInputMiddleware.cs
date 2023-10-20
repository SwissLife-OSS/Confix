using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Entities.Components;

namespace Confix.Tool.Middlewares;

public sealed class BuildComponentInputMiddleware : IMiddleware
{
    private readonly IComponentInputFactory _factory;

    public BuildComponentInputMiddleware(IComponentInputFactory factory)
    {
        _factory = factory;
    }

    /// <inheritdoc />
    public Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        var configuration = context.Features.Get<ConfigurationFeature>();

        var definitions = configuration.Component?.Inputs ??
            Array.Empty<ComponentInputDefinition>();

        var executor = ComponentInputExecutor.FromDefinitions(_factory, definitions);

        context.Features.Set(new ComponentInputExecutorFeature(executor));

        context.Logger.LoadedComponentInput();

        return next(context);
    }
}

file static class Log
{
    public static void LoadedComponentInput(this IConsoleLogger console)
    {
        console.Success("Component inputs loaded");
    }
}