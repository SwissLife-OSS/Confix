namespace Confix.Tool.Common.Pipelines;

public interface IMiddleware
{
    Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next);
}
