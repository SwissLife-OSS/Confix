namespace Confix.Tool.Commands.Logging;

public static class StringExtensions
{
    public static string AsHighlighted(this string value)
    {
        return $"[bold blue]{value}[/]";
    }

    public static string AsCommand(this string value)
    {
        return $"[bold blue]{value}[/]";
    }

    public static string AsQuestion(this string value)
    {
        return $"{Glyph.QuestionMark.ToMarkup()} {value}";
    }
}
