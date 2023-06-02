namespace Confix.Tool.Common.Pipelines;

public sealed class Pipeline
{
    private readonly MiddlewareDelegate _pipeline;

    public Pipeline(IServiceProvider services, MiddlewareDelegate pipeline)
    {
        Services = services;
        _pipeline = pipeline;
    }

    public IServiceProvider Services { get; }

    public async Task ExecuteAsync(IMiddlewareContext context)
    {
        await _pipeline(context);
    }
}
