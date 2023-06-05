using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Middlewares;

public class DelegateMiddleware : IMiddleware
{
    private readonly Func<IMiddlewareContext, MiddlewareDelegate, Task> _middlewareDelegate;

    public DelegateMiddleware(Func<IMiddlewareContext, MiddlewareDelegate, Task> middlewareDelegate)
    {
        _middlewareDelegate = middlewareDelegate;
    }

    public Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        return _middlewareDelegate(context, next);
    }
}