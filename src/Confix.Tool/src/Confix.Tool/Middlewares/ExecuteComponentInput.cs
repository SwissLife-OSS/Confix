using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Middlewares;

public sealed class ExecuteComponentInput : IMiddleware
{
    /// <inheritdoc />
    public Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        var configuration = context.Features.Get<ConfigurationFeature>();
        throw new NotImplementedException();
    }
}
