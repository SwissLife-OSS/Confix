using System.CommandLine;
using Confix.Extensions;
using Confix.Tool.Commands.Logging;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Confix.Tool.Common.Pipelines;

/// <summary>
/// Executes a pipeline, providing necessary parameters for the execution.
/// </summary>
/// <remarks>
/// <see cref="PipelineExecutor"/> acts as the executor for the created middleware pipeline. 
/// It takes a pipeline and parameters necessary for execution. 
/// </remarks>
/// <example>
/// Here's an example of how to use the `PipelineExecutor` class:
/// <code>
/// var pipelineExecutor = new PipelineExecutor(pipeline);
/// pipelineExecutor.AddParameter&lt;MySymbol>(mySymbol, value);
/// var exitCode = await pipelineExecutor.ExecuteAsync(CancellationToken.None);
/// </code>
/// </example>
public sealed class PipelineExecutor
{
    private readonly MiddlewareDelegate _pipeline;
    private readonly IServiceProvider _services;
    private readonly IDictionary<string, object> _contextData;
    private readonly Dictionary<Symbol, object?> _parameter = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineExecutor"/> class.
    /// </summary>
    public PipelineExecutor(
        MiddlewareDelegate pipeline,
        IServiceProvider services,
        IDictionary<string, object> contextData)
    {
        _pipeline = pipeline;
        _services = services;
        _contextData = contextData;
    }

    /// <summary>
    /// Adds a parameter to the pipeline execution context.
    /// </summary>
    /// <typeparam name="TParameter">The type of the parameter.</typeparam>
    /// <param name="parameter">The parameter symbol.</param>
    /// <param name="value">The value of the parameter.</param>
    /// <returns>The current pipeline executor instance.</returns>
    public PipelineExecutor AddParameter<TParameter>(TParameter parameter, object? value)
        where TParameter : Symbol
    {
        _parameter.Add(parameter, value);
        return this;
    }

    /// <summary>
    /// Executes the pipeline asynchronously.
    /// </summary>
    /// <param name="cancellationToken">
    /// A cancellation token to observe while waiting for the pipeline to complete.
    /// </param>
    /// <returns>The exit code of the pipeline execution.</returns>
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        var console = _services.GetRequiredService<IAnsiConsole>();

        IStatus status;
        if (_contextData.Get(Context.DisableStatus) is true)
        {
            status = new NullStatusContext();
        }
        else
        {
            var spectreStatus = new SpectreStatusContext(console);
            await spectreStatus.StartAsync(cancellationToken);
            status = spectreStatus;
        }

        var context = new MiddlewareContext
        {
            CancellationToken = cancellationToken,
            Parameter = ParameterCollection.From(_parameter),
            Console = _services.GetRequiredService<IAnsiConsole>(),
            Logger = _services.GetRequiredService<IConsoleLogger>(),
            Execution = _services.GetRequiredService<IExecutionContext>(),
            ContextData = _contextData,
            Status = status,
            Services = _services
        };

        await _pipeline(context);

        await status.StopAsync();

        return context.ExitCode;
    }
}

file class NullStatusContext : IStatus
{
    public string Message { get; set; } = "";

    public ValueTask<IAsyncDisposable> PauseAsync(CancellationToken cancellationToken)
        => ValueTask.FromResult<IAsyncDisposable>(this);
    public ValueTask StopAsync() => ValueTask.CompletedTask;

    /// <inheritdoc />
    public ValueTask DisposeAsync()
        => ValueTask.CompletedTask;
}