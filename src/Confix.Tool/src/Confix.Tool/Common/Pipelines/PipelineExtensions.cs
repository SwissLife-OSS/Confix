namespace Confix.Tool.Common.Pipelines;

/// <summary>
/// Common extensions for <see cref="PipelineBuilder"/>.
/// </summary>
public static class PipelineExtensions
{
    /// <summary>
    /// Builds the pipeline and returns a <see cref="PipelineExecutor"/> that can be used to
    /// execute the pipeline.
    /// </summary>
    /// <example>
    /// <code>
    /// var result = PipelineBuilder
    ///    .From(services);
    ///    .Use&lt;ExampleMiddleware1>()
    ///    .Use&lt;ExampleMiddleware2>()
    ///    .BuildExecutor()
    ///    .ExecuteAsync(context);
    /// </code>
    /// </example>
    public static PipelineExecutor BuildExecutor(this PipelineBuilder builder)
        => new(builder.Build());
}
