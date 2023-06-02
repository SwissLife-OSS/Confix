namespace Confix.Tool.Common.Pipelines;

/// <summary>
/// Represents a pipeline used to execute a specific command. A pipeline is a chain of middleware
/// components that are responsible for processing a command. This class is created by the
/// <see cref="PipelineBuilder"/> and is used to execute the pipeline.
/// </summary>
public sealed class Pipeline
{
    private readonly MiddlewareDelegate _pipeline;

    /// <summary>
    /// Initializes a new instance of the <see cref="Pipeline"/> class.
    /// </summary>
    /// <param name="services">
    /// The service provider used to resolve services needed by the pipeline.
    /// </param>
    /// <param name="pipeline">
    /// The delegate representing the sequence of middleware components in the pipeline.
    /// </param>
    public Pipeline(IServiceProvider services, MiddlewareDelegate pipeline)
    {
        Services = services;
        _pipeline = pipeline;
    }

    /// <summary>
    /// Gets the service provider used to resolve services needed by the pipeline.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Executes the pipeline using the provided middleware context.
    /// </summary>
    /// <param name="context">The context to use for the pipeline execution.</param>
    public async Task ExecuteAsync(IMiddlewareContext context)
    {
        await _pipeline(context);
    }
}
