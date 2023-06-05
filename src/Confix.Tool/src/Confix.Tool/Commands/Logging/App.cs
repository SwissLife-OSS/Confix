namespace Confix.Tool.Commands.Logging;

public static class App
{
    public static IConsoleLogger Log { get; internal set; } = ConsoleLogger.NullLogger;
}
