using Spectre.Console;

namespace Confix.Tool.Commands.Logging;

public static class Styles
{
    public static Style Error { get; } = new(foreground: Color.Red, decoration: Decoration.Bold);

    public static Style Dimmed { get; } = new(decoration: Decoration.Dim);
}
