using Spectre.Console;

namespace Confix.Tool.Commands.Logging;

public sealed class ConsoleLogger : IConsoleLogger
{
    private readonly IAnsiConsole _console;
    private Verbosity _minVerbosity;

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

    public IDisposable SetVerbosity(Verbosity verbosity)
    {
        return new VerbosityScope(this, verbosity);
    }

    public static IConsoleLogger NullLogger => new NullLogger();

    private sealed class VerbosityScope : IDisposable
    {
        private readonly ConsoleLogger _logger;
        private readonly Verbosity _previousVerbosity;

        public VerbosityScope(ConsoleLogger logger, Verbosity verbosity)
        {
            _logger = logger;
            _previousVerbosity = _logger._minVerbosity;
            _logger._minVerbosity = verbosity;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _logger._minVerbosity = _previousVerbosity;
        }
    }
}

file class NullLogger : IConsoleLogger
{
    /// <inheritdoc />
    public void Log(ref ILoggerMessage message)
    {
        // empty on purpose
    }

    /// <inheritdoc />
    public IDisposable SetVerbosity(Verbosity verbosity)
    {
        return new NullScope();
    }

    private sealed class NullScope : IDisposable
    {
        /// <inheritdoc />
        public void Dispose()
        {
            // empty on purpose
        }
    }
}
