using Spectre.Console;

namespace Confix.Tool.Commands.Logging;

public readonly record struct JsonLoggerMessage : ILoggerMessage
{
    public required string Message { get; init; }

    public required Verbosity Verbosity { get; init; }

    /// <inheritdoc />
    public void WriteTo(IAnsiConsole console)
    {
        console.WriteJson(Message);
    }
}
