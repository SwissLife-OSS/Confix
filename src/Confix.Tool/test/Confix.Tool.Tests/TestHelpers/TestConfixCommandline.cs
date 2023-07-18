using System.CommandLine.Parsing;
using Confix.Tool;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using ExecutionContext = Confix.Tool.Common.Pipelines.ExecutionContext;

namespace ConfiX.Inputs;

public class TestConfixCommandline : IDisposable
{
    private readonly InMemoryConsoleLogger _consoleLogger;

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

    /// <inheritdoc />
    public void Dispose()
    {
        Directories.Dispose();
        Console.Dispose();
    }
}
