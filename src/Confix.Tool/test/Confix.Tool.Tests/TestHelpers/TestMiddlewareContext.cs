using ConfiX.Inputs;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using ExecutionContext = Confix.Tool.Common.Pipelines.ExecutionContext;

namespace ConfiX.Entities.Component.Configuration.Middlewares;

public class TestMiddlewareContext : IMiddlewareContext, IDisposable
{
    public TestMiddlewareContext()
    {
        MutableParameterCollection = new MutableParameterCollection();
        ConsoleLogger = new InMemoryConsoleLogger();
        Directories = new ExecutionDirectories();
        Features = new FeatureCollection();
        ContextData = new Dictionary<string, object>();
        Execution = new ExecutionContext(Directories.Content.FullName, Directories.Home.FullName);
        Status = new TestStatus();
        Services = new ServiceCollection().BuildServiceProvider();
    }

    public MutableParameterCollection MutableParameterCollection { get; set; }

    public InMemoryConsoleLogger ConsoleLogger { get; set; }

    public ExecutionDirectories Directories { get; set; }

    /// <inheritdoc />
    public IFeatureCollection Features { get; set; }

    /// <inheritdoc />
    public IParameterCollection Parameter => MutableParameterCollection;

    /// <inheritdoc />
    public IDictionary<string, object> ContextData { get; set; }

    /// <inheritdoc />
    public IAnsiConsole Console => ConsoleLogger.Console;

    /// <inheritdoc />
    public IConsoleLogger Logger => ConsoleLogger;

    /// <inheritdoc />
    public CancellationToken CancellationToken => CancellationToken.None;

    /// <inheritdoc />
    public IExecutionContext Execution { get; set; }

    /// <inheritdoc />
    public int ExitCode { get; set; }

    /// <inheritdoc />
    public IStatus Status { get; set; }

    /// <inheritdoc />
    public IServiceProvider Services { get; set; }

    /// <inheritdoc />
    public void Dispose()
    {
        Directories.Dispose();
    }
}
