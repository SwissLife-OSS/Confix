using Confix.Tool.Commands.Logging;
using Spectre.Console;

namespace Confix.Tool.Common.Pipelines;

public sealed record MiddlewareContext : IMiddlewareContext
{
    public IFeatureCollection Features { get; init; } = new FeatureCollection();

    /// <inheritdoc />
    public IDictionary<string, object> ContextData { get; init; } =
        new Dictionary<string, object>();

    /// <inheritdoc />
    public required IConsoleLogger Logger { get; init; }

    public required CancellationToken CancellationToken { get; init; }

    /// <inheritdoc />
    public required IExecutionContext Execution { get; init; }

    /// <inheritdoc />
    public required IAnsiConsole Console { get; init; }

    /// <inheritdoc />
    public required IParameterCollection Parameter { get; init; }

    /// <inheritdoc />
    public required IStatus Status { get; init; }

    /// <inheritdoc />
    public int ExitCode { get; set; } = ExitCodes.Success;
}
