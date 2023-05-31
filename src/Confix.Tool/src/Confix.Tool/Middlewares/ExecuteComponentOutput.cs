using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Middlewares;

public sealed class ExecuteComponentOutput : IMiddleware
{
    /// <inheritdoc />
    public Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        throw new NotImplementedException();
    }
}
