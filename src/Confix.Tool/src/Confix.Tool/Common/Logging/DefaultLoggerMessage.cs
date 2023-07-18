using Spectre.Console;

namespace Confix.Tool.Commands.Logging;

public readonly record struct DefaultLoggerMessage : ILoggerMessage
{
    public required string Template { get; init; }

    public required Verbosity Verbosity { get; init; }

    public object[]? Arguments { get; init; }

    public Glyph? Glyph { get; init; }

    public Style? Style { get; init; }

    public Exception? Exception { get; init; }

    /// <inheritdoc />
    public void WriteTo(IAnsiConsole console)
    {
        var message = Template;

        if (Glyph is not null)
        {
            message = $"{Glyph.Value.ToMarkup()} {message}";
        }
        else
        {
            message = $"  {message}";
        }

        var formatted = string.Format(message, Arguments ?? Array.Empty<object>());
        // markup is by default not wrapped, so we need to add a newline 
        formatted += Environment.NewLine;

        var markup = new Markup(formatted, Style ?? Style.Plain);

        console.Write(markup);

        if (Exception is not null)
        {
            console.WriteException(Exception);
        }
    }
}
