using Spectre.Console;

namespace Confix.Tool;

public static class DefaultConsole
{
    public static IAnsiConsole Create()
    {
        var console = AnsiConsole.Create(new AnsiConsoleSettings());
        console.Profile.Width = 500;

        return console;
    }
}
