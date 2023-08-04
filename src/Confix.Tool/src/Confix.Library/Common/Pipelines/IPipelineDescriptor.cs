using System.CommandLine;

namespace Confix.Tool.Common.Pipelines;

public interface IPipelineDescriptor
{
    /// <summary>
    /// Adds a middleware component to the pipeline.
    /// </summary>
    /// <typeparam name="TMiddleware">The type of the middleware component.</typeparam>
    /// <returns>The current command pipeline builder instance.</returns>
    IPipelineDescriptor Use<TMiddleware>() where TMiddleware : IMiddleware;

    /// <summary>
    /// Adds a middleware component to the pipeline.
    /// </summary>
    /// <param name="middleware">The middleware to add</param>
    /// <returns>The current command pipeline builder instance.</returns>
    IPipelineDescriptor Use<TMiddleware>(TMiddleware middleware)
        where TMiddleware : IMiddleware;

    /// <summary>
    /// Adds a middleware component to the pipeline.
    /// </summary>
    IPipelineDescriptor Use<TMiddleware>(Func<IServiceProvider, TMiddleware> middlewareFactory)
        where TMiddleware : IMiddleware;

    /// <summary>
    /// Adds an argument to the command and maps it to the <see cref="IParameterCollection"/>.
    /// </summary>
    /// <typeparam name="TArgument">The type of the argument.</typeparam>
    /// <param name="argument">The argument to add.</param>
    /// <returns>The current command pipeline builder instance.</returns>
    IPipelineDescriptor AddArgument<TArgument>(TArgument argument)
        where TArgument : Argument;

    /// <summary>
    /// Adds an option to the command and maps it to the <see cref="IParameterCollection"/>.
    /// </summary>
    /// <typeparam name="TOption">The type of the option.</typeparam>
    /// <param name="option">The option to add.</param>
    /// <returns>The current command pipeline builder instance.</returns>
    IPipelineDescriptor AddOption<TOption>(TOption option)
        where TOption : Option;
}
