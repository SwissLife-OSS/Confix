using Confix.Tool.Middlewares;

namespace Confix.Tool.Common.Pipelines;

public static class CommandPipelineBuilderExtensions
{
    public static CommandPipelineBuilder UseHandler(
        this CommandPipelineBuilder builder,
        Func<IMiddlewareContext, Task> action)
    {
        builder.Use(new DelegateMiddleware((context, _) => action(context)));

        return builder;
    }
}