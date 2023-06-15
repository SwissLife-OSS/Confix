using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool.Common.Pipelines;

public sealed class PipelineDescriptor
    : IPipelineDescriptor
{
    public PipelineDefinition Definition { get; } = new();

    /// <inheritdoc />
    public IPipelineDescriptor Use<TMiddleware>() where TMiddleware : IMiddleware
    {
        Definition.Middlewares.Add(sp => sp.GetRequiredService<TMiddleware>());

        return this;
    }

    /// <inheritdoc />
    public IPipelineDescriptor Use<TMiddleware>(TMiddleware middleware)
        where TMiddleware : IMiddleware
    {
        Definition.Middlewares.Add(_ => middleware);

        return this;
    }

    /// <inheritdoc />
    public IPipelineDescriptor Use<TMiddleware>(
        Func<IServiceProvider, TMiddleware> middlewareFactory)
        where TMiddleware : IMiddleware
    {
        Definition.Middlewares.Add(sp => middlewareFactory(sp));

        return this;
    }

    /// <inheritdoc />
    public IPipelineDescriptor AddArgument<TArgument>(TArgument argument)
        where TArgument : Argument
    {
        Definition.Arguments.Add(argument);

        return this;
    }

    /// <inheritdoc />
    public IPipelineDescriptor AddOption<TOption>(TOption option)
        where TOption : Option
    {
        Definition.Options.Add(option);

        return this;
    }
}
