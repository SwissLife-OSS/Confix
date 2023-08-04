namespace Confix.Tool.Common.Pipelines;

/// <summary>
/// Represents a middleware component within a pipeline. Middleware components are responsible for
/// executing specific parts of the pipeline and can pass execution information down the pipeline.
/// </summary>
/// <example>
/// Here's an example of how to implement an `IMiddleware`:
/// <code>
/// public class ExampleMiddleware : IMiddleware
/// {
///     public async Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
///     {
///         // Pre-invocation code here
///
///         // Invoke the next middleware in the pipeline
///         await next(context);
///
///         // Post-invocation code here
///     }
/// }
/// </code>
/// This middleware does nothing by itself but demonstrates the typical structure of a middleware component.
/// </example>
public interface IMiddleware
{
    /// <summary>
    /// Executes the middleware operation.
    /// </summary>
    /// <param name="context">
    /// The context of the middleware which includes parameters, execution information and other contextual data.
    /// </param>
    /// <param name="next">
    /// The delegate to the next middleware in the pipeline.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that completes when the middleware has finished processing.
    /// </returns>
    Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next);
}
