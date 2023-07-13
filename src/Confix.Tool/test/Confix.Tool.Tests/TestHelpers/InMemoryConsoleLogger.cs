using Confix.Tool.Commands.Logging;
using Spectre.Console.Testing;

namespace ConfiX.Inputs;

public sealed class InMemoryConsoleLogger : IConsoleLogger
{
    public TestConsole Console { get; } = new()
    {
        Profile = { Width = 100000 }
    };

    /// <inheritdoc />
    public void Log(ref ILoggerMessage message)
    {
        message.WriteTo(Console);
    }
}
