using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using Confix.Tool.Commands.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace Confix.Tool;

public static class ExceptionHandlerCommandLineBuilderExtensions
{
    public static CommandLineBuilder AddExceptionHandler(this CommandLineBuilder builder)
    {
        builder.AddMiddleware(ExceptionMiddleware, MiddlewareOrder.ExceptionHandler);

        return builder;
    }

    private static async Task ExceptionMiddleware(
        InvocationContext context,
        Func<InvocationContext, Task> next)
    {
        try
        {
            await next(context);
        }
        catch (AggregateException exception) when (exception.InnerExceptions.Any(e => e is ExitException or ValidationException))
        {
            foreach (var innerException in exception.InnerExceptions)
            {
                context.HandleException(innerException);
            }
        }
        catch (Exception exception)
        {
            context.HandleException(exception);
        }
    }


}

file static class LogExtensions
{
    public static void HandleException(this InvocationContext context, Exception exception)
    {
        var logger = context.BindingContext.GetRequiredService<IConsoleLogger>();
        var console = context.BindingContext.GetRequiredService<IAnsiConsole>();

        switch (exception)
        {
            case OperationCanceledException or TaskCanceledException:
                // ignored on purpose
                return;
            case ExitException exitException:
                logger.ExitException(exitException);
                console.PrintHelp(exitException);
                break;
            case ValidationException validationException:
                logger.ValidationException(validationException);
                console.PrintValidationError(validationException);
                break;
            default:
                logger.UnhandledException(exception);
                throw exception;
        }

        context.ExitCode = ExitCodes.Error;
    }
    public static void ExitException(this IConsoleLogger logger, ExitException exception)
    {
        logger.Error("Confix failed.");
        logger.Information($"[red]{exception.Message}[/]");
        logger.TraceException(exception);
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

    public static void ValidationException(this IConsoleLogger logger, ValidationException exception)
    {
        logger.Error("Confix failed due to faulty configuration.");
        logger.TraceException(exception);
    }

    public static void PrintValidationError(this IAnsiConsole console, ValidationException exception)
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
        logger.Exception("Confix terminated unexpectedly.", exception);
    }
}