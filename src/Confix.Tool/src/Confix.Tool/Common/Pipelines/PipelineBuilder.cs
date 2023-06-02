using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool.Common.Pipelines;

public sealed class PipelineBuilder
{
    private readonly IServiceProvider _services;
    private readonly List<Func<IServiceProvider, IMiddleware>> _middlewareFactories = new();

    public PipelineBuilder(IServiceProvider services)
    {
        _services = services;
    }

    public PipelineBuilder Use<TMiddleware>() where TMiddleware : IMiddleware
    {
        _middlewareFactories.Add(sp => sp.GetRequiredService<TMiddleware>());

        return this;
    }

    public Pipeline Build()
    {
        MiddlewareDelegate next = _ => Task.CompletedTask;

        for (var i = _middlewareFactories.Count - 1; i >= 0; i--)
        {
            var middleware = _middlewareFactories[i](_services);
            var current = next;
            next = context => middleware.InvokeAsync(context, current);
        }

        return new Pipeline(_services, next);
    }

    public static PipelineBuilder From(IServiceProvider serviceProvider)
    {
        return new(serviceProvider);
    }
}
