using Confix.Tool.Middlewares;
using Microsoft.Extensions.DependencyInjection;

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

    public static CommandPipelineBuilder UseHandler<T1>(
        this CommandPipelineBuilder builder,
        Func<IMiddlewareContext, T1, Task> action) where T1 : notnull
    {
        builder.Use(sp =>
            new DelegateMiddleware((context, _) => action(context, sp.GetRequiredService<T1>())));

        return builder;
    }

    public static CommandPipelineBuilder UseHandler<T1, T2>(
        this CommandPipelineBuilder builder,
        Func<IMiddlewareContext, T1, T2, Task> action)
    {
        builder.Use(sp => new DelegateMiddleware((context, _)
            => action(
                context,
                sp.GetRequiredService<T1>(),
                sp.GetRequiredService<T2>())));

        return builder;
    }
}
