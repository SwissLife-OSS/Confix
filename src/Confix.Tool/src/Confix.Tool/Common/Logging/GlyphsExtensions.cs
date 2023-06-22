using Spectre.Console;

namespace Confix.Tool.Commands.Logging;

public static class GlyphsExtensions
{
    public static string ToMarkup(this Glyph glyph)
        => glyph switch
        {
            Glyph.Check => "[green bold]✓[/]",
            Glyph.Cross => "[red bold]✕[/]",
            Glyph.QuestionMark => "[lime bold]?[/]",
            Glyph.ExlamationMark => "[yellow bold]![/]",
            _ => throw new ArgumentOutOfRangeException(nameof(glyph), glyph, null)
        };
}
