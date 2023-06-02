namespace Confix.Tool.Common.Pipelines;

/// <summary>
/// Represents a delegate that handles the execution of a middleware component in the pipeline.
/// </summary>
/// <remarks>
/// A `MiddlewareDelegate` is used to invoke a middleware's processing logic and pass control to the
/// next middleware in the pipeline. The middleware's `InvokeAsync` method typically calls the
/// `MiddlewareDelegate` once it has completed its pre-invocation processing. It might also call it
/// afterwards, or not at all, depending on the intended behavior.
/// </remarks>
/// <example>
/// Here's an example of how to use the `MiddlewareDelegate` in a middleware component:
/// <code lang="csharp">
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
/// In this example, the `next` delegate, an instance of `MiddlewareDelegate`, is invoked to pass
/// control to the next middleware in the pipeline.
/// </example>
/// <param name="context">The middleware context for the current middleware execution.</param>
public delegate Task MiddlewareDelegate(IMiddlewareContext context);
