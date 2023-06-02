using System.CommandLine.Invocation;
using Spectre.Console;

namespace Confix.Tool.Common.Pipelines;

/// <summary>
/// Represents the context that is passed from middleware to middleware. This context contains
/// essential information about the execution, console, parameters, etc., which can be utilized by
/// the middleware components.
/// </summary>
/// <remarks>
/// The `IMiddlewareContext` serves as a communication medium for middleware components in a
/// pipeline. Each middleware can read or modify the context, providing a mechanism for data
/// transfer and alteration across different stages of pipeline execution. Notably, the
/// `IMiddlewareContext` carries console details, cancellation tokens, and execution context,
/// facilitating the pipeline to interact with the environment and handle cancellation requests.
/// </remarks>
public interface IMiddlewareContext
{
    /// <summary>
    /// Represents a collection of features added and consumed by the pipeline.
    /// </summary>
    IFeatureCollection Features { get; }

    /// <summary>
    /// Represents a collection of parameters that can be passed along the pipeline.
    /// </summary>
    IParameterCollection Parameter { get; }

    /// <summary>
    /// Represents a dictionary of context data that can be passed along the pipeline.
    /// </summary>
    IDictionary<string, object> ContextData { get; }

    /// <summary>
    /// Represents the console that the pipeline can write to.
    /// </summary>
    IAnsiConsole Console { get; }

    /// <summary>
    /// Represents the cancellation token that the pipeline can use to check if cancellation has
    /// been requested.
    /// </summary>
    CancellationToken CancellationToken { get; }

    /// <summary>
    /// Represents the execution context of the current pipeline execution.
    /// </summary>
    IExecutionContext Execution { get; }

    /// <summary>
    /// Represents the exit code of the current pipeline execution. This value is returned to the
    /// command line after the pipeline has finished executing.
    /// </summary>
    int ExitCode { get; set; }
}
