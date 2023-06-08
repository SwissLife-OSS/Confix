using Spectre.Console;

namespace Confix.Tool.Commands.Logging;

public interface ILoggerMessage
{
    Verbosity Verbosity { get; }

    void WriteTo(IAnsiConsole console);
}
