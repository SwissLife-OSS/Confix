namespace Confix.Tool.Commands.Logging;

public static class App
{
    private static readonly AsyncLocal<IConsoleLogger> _log = new();

    public static IConsoleLogger Log
    {
        get => _log.Value ?? ConsoleLogger.NullLogger;
        set => _log.Value = value;
    }
}
