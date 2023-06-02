using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool.Common.Pipelines;

/// <summary>
/// Builds a middleware pipeline that can be used to execute commands.
/// </summary>
/// <remarks>
/// `PipelineBuilder` provides a way to setup a chain of middleware components that are used to
/// process a command.
/// Middleware components are added to the pipeline using the `Use` method. The middleware
/// components are invoked in the order they were added.
/// </remarks>
/// <example>
/// <p>
/// Here's an example of how to use the `PipelineBuilder` class:
/// </p>
/// <code>
/// var pipeLine = PipelineBuilder
///    .From(services);
///    .Use&lt;ExampleMiddleware1>()
///    .Use&lt;ExampleMiddleware2>()
///    .Build();
/// 
/// </code>
/// <p>
/// This example creates a `PipelineBuilder`, adds two middleware components to it, and then builds
/// a <see cref="Pipeline"/>.
/// </p>
/// </example>
public sealed class PipelineBuilder
{
    private readonly IServiceProvider _services;
    private readonly List<Func<IServiceProvider, IMiddleware>> _middlewareFactories = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineBuilder"/> class.
    /// </summary>
    public PipelineBuilder(IServiceProvider services)
    {
        _services = services;
    }

    /// <summary>
    /// Adds a middleware of type <typeparamref name="TMiddleware"/> to the pipeline.
    /// </summary>
    /// <typeparam name="TMiddleware">The type of the middleware.</typeparam>
    /// <returns>The current pipeline builder instance.</returns>
    public PipelineBuilder Use<TMiddleware>() where TMiddleware : IMiddleware
    {
        _middlewareFactories.Add(sp => sp.GetRequiredService<TMiddleware>());

        return this;
    }

    /// <summary>
    /// Builds the <see cref="Pipeline"/> with the configured middleware components.
    /// </summary>
    /// <returns>A new instance of <see cref="Pipeline"/>.</returns>
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

    /// <summary>
    /// Creates a new instance of <see cref="PipelineBuilder"/>.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>A new instance of <see cref="PipelineBuilder"/>.</returns>
    public static PipelineBuilder From(IServiceProvider serviceProvider)
    {
        return new(serviceProvider);
    }
}
