using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Middlewares;

public interface IComponentInputExecutor
{
    Task ExecuteAsync(IMiddlewareContext context);
}
