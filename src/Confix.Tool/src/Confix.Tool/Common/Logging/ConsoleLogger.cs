using Spectre.Console;

namespace Confix.Tool.Commands.Logging;

public sealed class ConsoleLogger : IConsoleLogger
{
    private readonly IAnsiConsole _console;
    private readonly Verbosity _minVerbosity;

    public ConsoleLogger(IAnsiConsole console, Verbosity minVerbosity)
    {
        _console = console;
        _minVerbosity = minVerbosity;
    }

    /// <inheritdoc />
    public void Log(ref ILoggerMessage message)
    {
        if (message.Verbosity >= _minVerbosity)
        {
            message.WriteTo(_console);
        }
    }

    public static IConsoleLogger NullLogger => new NullLogger();
}

file class NullLogger : IConsoleLogger
{
    /// <inheritdoc />
    public void Log(ref ILoggerMessage message)
    {
    }
}
