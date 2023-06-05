using Confix.Tool.Common.Pipelines;
using Confix.Tool.Entities.Component;

namespace Confix.Tool.Middlewares;

public sealed class ExecuteComponentInputMiddleware : IMiddleware
{
    private readonly IComponentInputFactory _factory;

    public ExecuteComponentInputMiddleware(IComponentInputFactory factory)
    {
        _factory = factory;
    }

    /// <inheritdoc />
    public Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        var configuration = context.Features.Get<ConfigurationFeature>();

        return Task.CompletedTask;
    }
}
