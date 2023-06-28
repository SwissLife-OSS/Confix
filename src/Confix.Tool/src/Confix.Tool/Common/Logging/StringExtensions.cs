using Spectre.Console;

namespace Confix.Tool.Commands.Logging;

public static class StringExtensions
{
    public static string AsHighlighted(this string value)
    {
        return value.WithColor(Color.Blue).AsBold();
    }

    public static string AsCommand(this string value)
    {
        return value.WithColor(Color.Aqua).AsBold();
    }

    public static string AsPath(this string value)
    {
        return value.EscapeMarkup().WithColor(Color.Yellow).AsBold();
    }

    public static string AsError(this string value)
        => $"[red bold]{Glyph.Cross.ToMarkup()}[/] {value.EscapeMarkup()}";

    public static string AsVariableValue(this string value)
        => value.EscapeMarkup().WithColor(Color.Yellow);

    public static string AsVariableName(this string value)
        => value.WithColor(Color.Green);

    public static string WithColor(this string value, Color color)
    {
        return $"[#{color.ToHex()}]{value}[/]";
    }

    public static string AsBold(this string value)
    {
        return $"[bold]{value}[/]";
    }
}
