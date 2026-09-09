using Confix.Tool.Commands.Logging;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Confix.Tool;

public static class ExceptionHandlerCommandLineBuilderExtensions
{
    /// <summary>
    /// Executes <paramref name="action" /> and renders any exception that occurred.
    /// </summary>
    /// <returns>The exit code of the execution.</returns>
    public static async Task<int> ExecuteWithExceptionHandlingAsync(
        this IServiceProvider services,
        Func<Task<int>> action)
    {
        try
        {
            return await action();
        }
        catch (AggregateException exception) when (
            exception.InnerExceptions.Any(e => e is ExitException or ValidationException))
        {
            foreach (var innerException in exception.InnerExceptions)
            {
                services.HandleException(innerException);
            }

            return ExitCodes.Error;
        }
        catch (Exception exception)
        {
            return services.HandleException(exception);
        }
    }
}

file static class LogExtensions
{
    public static int HandleException(this IServiceProvider services, Exception exception)
    {
        var logger = services.GetRequiredService<IConsoleLogger>();
        var console = services.GetRequiredService<IAnsiConsole>();

        switch (exception)
        {
            case OperationCanceledException or TaskCanceledException:
                // ignored on purpose
                return ExitCodes.Error;

            case ExitException exitException:
                logger.ExitException(exitException);
                console.PrintHelp(exitException);
                console.PrintDetails(exitException);
                break;

            case ValidationException validationException:
                logger.ValidationException(validationException);
                console.PrintValidationError(validationException);
                break;

            default:
                logger.UnhandledException(exception);
                break;
        }

        return ExitCodes.Error;
    }

    public static void ExitException(this IConsoleLogger logger, ExitException exception)
    {
        logger.Error("Confix failed.");
        logger.Information($"[red]{exception.Message}[/]");
        logger.TraceException(exception.InnerException ?? exception);
    }

    public static void PrintHelp(this IAnsiConsole console, ExitException exception)
    {
        if (exception.Help is not null)
        {
            var panel = new Panel(exception.Help)
            {
                Header = new PanelHeader($"{Emoji.Known.LightBulb} Help", Justify.Left),
            };
            console.Write(panel);
        }
    }
    
    public static void PrintDetails(this IAnsiConsole console, ExitException exception)
    {
        if (exception.Details is not null)
        {
            console.MarkupLine($"[yellow]{exception.Details.EscapeMarkup()}[/]");
        }
    }
    
    public static void ValidationException(
        this IConsoleLogger logger,
        ValidationException exception)
    {
        logger.Error("Confix failed due to faulty configuration.");
        logger.TraceException(exception);
    }

    public static void PrintValidationError(
        this IAnsiConsole console,
        ValidationException exception)
    {
        var validationErrorTree = new Tree($"[red]{exception.Message}[/]");
        foreach (var error in exception.Errors)
        {
            validationErrorTree.AddNode($"[red]{error}[/]");
        }

        console.Write(validationErrorTree);
    }

    public static void UnhandledException(this IConsoleLogger logger, Exception exception)
    {
        logger.Error("Confix terminated unexpectedly. \n  {0}", exception.Message);
        logger.Exception("Exception: ", exception);
    }
}
