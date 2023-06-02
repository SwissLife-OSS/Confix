using System.CommandLine.Invocation;
using Spectre.Console;

namespace Confix.Tool.Common.Pipelines;

public interface IMiddlewareContext
{
    IFeatureCollection Features { get; }

    IParameterCollection Parameter { get; }

    IDictionary<string, object> ContextData { get; }

    IAnsiConsole Console { get; }

    CancellationToken CancellationToken { get; }

    IExecutionContext Execution { get; }
}