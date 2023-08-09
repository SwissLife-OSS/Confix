using System.CommandLine;

namespace Confix.Tool.Common.Pipelines;

public abstract class Pipeline
{
    private IReadOnlyList<Func<IServiceProvider, IMiddleware>> _middlewares =
        Array.Empty<Func<IServiceProvider, IMiddleware>>();

    protected abstract void Configure(IPipelineDescriptor builder);

    protected Pipeline()
    {
        Initialize();
    }

    private void Initialize()
    {
        var descriptor = new PipelineDescriptor();
        Configure(descriptor);

        Arguments = descriptor.Definition.Arguments;
        Options = descriptor.Definition.Options;
        ContextData = descriptor.Definition.ContextData;
        _middlewares = descriptor.Definition.Middlewares;
    }

    public IDictionary<string, object> ContextData { get; set; }

    public IReadOnlySet<Argument> Arguments { get; private set; } = new HashSet<Argument>();

    public IReadOnlySet<Option> Options { get; private set; } = new HashSet<Option>();

    public PipelineExecutor BuildExecutor(IServiceProvider services)
    {
        return new PipelineExecutor(BuildDelegate(services), services, ContextData);
    }

    public async Task ExecuteAsync(IMiddlewareContext context)
    {
        await BuildDelegate(context.Services)(context);
    }

    /// <summary>
    /// Builds the <see cref="Pipeline"/> with the configured middleware components.
    /// </summary>
    /// <returns>A new instance of <see cref="Pipeline"/>.</returns>
    private MiddlewareDelegate BuildDelegate(IServiceProvider services)
    {
        MiddlewareDelegate next = _ => Task.CompletedTask;

        for (var i = _middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = _middlewares[i](services);
            var current = next;
            next = async context =>
            {
                var status = context.Status.Message;

                await middleware.InvokeAsync(context, current);

                context.Status.Message = status;
            };
        }

        return next;
    }
}
