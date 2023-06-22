using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Confix.Tool.Commands.Logging;

public static class ConsoleLoggerExtensions
{
    public static void Success(
        this IConsoleLogger logger,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string message,
        params object[] arguments)
    {
        ILoggerMessage loggerMessage = new DefaultLoggerMessage
        {
            Verbosity = Verbosity.Normal,
            Template = message,
            Arguments = arguments,
            Glyph = Glyph.Check
        };

        logger.Log(ref loggerMessage);
    }

    public static void Information(
        this IConsoleLogger logger,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string message,
        params object[] arguments)
    {
        ILoggerMessage loggerMessage = new DefaultLoggerMessage
        {
            Verbosity = Verbosity.Normal,
            Template = message,
            Arguments = arguments
        };

        logger.Log(ref loggerMessage);
    }

    public static void Debug(
        this IConsoleLogger logger,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string message,
        params object[] arguments)
    {
        ILoggerMessage loggerMessage = new DefaultLoggerMessage
        {
            Template = message,
            Arguments = arguments,
            Verbosity = Verbosity.Detailed,
            Style = Styles.Dimmed
        };

        logger.Log(ref loggerMessage);
    }

    public static void Trace(
        this IConsoleLogger logger,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string message,
        params object[] arguments)
    {
        ILoggerMessage loggerMessage = new DefaultLoggerMessage
        {
            Template = message,
            Arguments = arguments,
            Verbosity = Verbosity.Diagnostic,
            Style = Styles.Dimmed
        };

        logger.Log(ref loggerMessage);
    }

    public static void Warning(
        this IConsoleLogger logger,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string message,
        params object[] arguments)
    {
        ILoggerMessage loggerMessage = new DefaultLoggerMessage
        {
            Template = message,
            Arguments = arguments,
            Verbosity = Verbosity.Minimal,
            Glyph = Glyph.ExlamationMark
        };

        logger.Log(ref loggerMessage);
    }

    public static void Error(
        this IConsoleLogger logger,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string message,
        params object[] arguments)
    {
        ILoggerMessage loggerMessage = new DefaultLoggerMessage
        {
            Template = message,
            Arguments = arguments,
            Verbosity = Verbosity.Quiet,
            Style = Styles.Error,
            Glyph = Glyph.Cross
        };

        logger.Log(ref loggerMessage);
    }

    public static void Exception(
        this IConsoleLogger logger,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string message,
        Exception exception,
        params object[] arguments)
    {
        ILoggerMessage loggerMessage = new DefaultLoggerMessage
        {
            Template = message,
            Arguments = arguments,
            Verbosity = Verbosity.Quiet,
            Style = Styles.Error,
            Glyph = Glyph.Cross,
            Exception = exception
        };

        logger.Log(ref loggerMessage);
    }

     public static void TraceException(
        this IConsoleLogger logger,
        Exception exception,
        params object[] arguments)
    {
        ILoggerMessage loggerMessage = new DefaultLoggerMessage
        {
            Template = string.Empty,
            Arguments = arguments,
            Verbosity = Verbosity.Diagnostic,
            Style = Styles.Error,
            Exception = exception
        };

        logger.Log(ref loggerMessage);
    }
}
