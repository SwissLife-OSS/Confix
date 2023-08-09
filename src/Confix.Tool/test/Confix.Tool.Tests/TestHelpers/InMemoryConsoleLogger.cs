using Confix.Tool;
using Confix.Tool.Commands.Logging;
using Spectre.Console;
using Spectre.Console.Testing;

namespace Confix.Inputs;

public sealed class InMemoryConsoleLogger : IConsoleLogger
{
    public TestConsole Console { get; } = new()
    {
        Profile = { Width = 100000 }
    };

    /// <inheritdoc />
    public void Log(ref ILoggerMessage message)
    {
        if (message is DefaultLoggerMessage { Exception: { } exception } exceptionMessage)
        {
            Console.WriteLine($"Exception of type {exception.GetType().Name} was thrown.");
            // we remove the exception so we do not log the stacktrace
            message = exceptionMessage with { Exception = null };
        }

        message.WriteTo(Console);
    }

    /// <inheritdoc />
    public IDisposable SetVerbosity(Verbosity verbosity)
    {
        return NullDisposable.Instance;
    }

    private sealed class NullDisposable : IDisposable
    {
        public static NullDisposable Instance { get; } = new();

        /// <inheritdoc />
        public void Dispose()
        {
            // empty on purpose
        }
    }
}
