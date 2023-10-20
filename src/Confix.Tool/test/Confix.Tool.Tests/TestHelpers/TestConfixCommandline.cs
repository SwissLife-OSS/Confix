using System.CommandLine.Parsing;
using Confix.Tool;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using ExecutionContext = Confix.Tool.Common.Pipelines.ExecutionContext;

namespace Confix.Inputs;

public class TestConfixCommandline : IDisposable
{
    private InMemoryConsoleLogger _consoleLogger;

    public TestConfixCommandline() : this(_ => { })
    {
    }

    public TestConfixCommandline(Action<ConfixCommandLine> configure)
    {
        Console = new ConfixTestConsole();
        Directories = new ExecutionDirectories();
        ExecutionContext =
            new ExecutionContext(Directories.Content.FullName, Directories.Home.FullName);

        _consoleLogger = new InMemoryConsoleLogger();

        var cli = new ConfixCommandLine();
        cli.AddTestService<IAnsiConsole>(_ => Console);
        cli.AddTestService<IExecutionContext>(_ => ExecutionContext);
        cli.AddTestService<IConsoleLogger>(_ => _consoleLogger);
        configure(cli);

        Parser = cli.Build();
    }

    public ExecutionDirectories Directories { get; }

    public ConfixTestConsole Console { get; }

    public string Output => _consoleLogger.Console.Output;

    public ExecutionContext ExecutionContext { get; set; }

    public Parser Parser { get; }

    public async Task RunAsync(string args) => await Parser.InvokeAsync(args.Split(" "), Console);

    public async Task RunAsync(params string[] args) => await Parser.InvokeAsync(args, Console);

    public void ResetConsole() => _consoleLogger = new InMemoryConsoleLogger();

    /// <inheritdoc />
    public void Dispose()
    {
        Directories.Dispose();
        Console.Dispose();
    }
}
