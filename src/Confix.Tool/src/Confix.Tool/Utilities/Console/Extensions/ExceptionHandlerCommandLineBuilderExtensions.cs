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
        catch (AggregateException exception) when (exception.InnerExceptions.Any(e => e is ExitException))
        {
            context.ExitCode = ExitCodes.Error;

            var logger = context.BindingContext.GetRequiredService<IConsoleLogger>();
            var console = context.BindingContext.GetRequiredService<IAnsiConsole>();
            foreach (var innerException in exception.InnerExceptions)
            {
                if (innerException is ExitException exitException)
                {
                    logger.ExitException(exitException);
                    console.PrintHelp(exitException);
                }
                else
                {
                    logger.UnhandledException(innerException);
                }
            }
        }
        catch (ExitException exception) when (exception is { Message: var message })
        {
            context.ExitCode = ExitCodes.Error;

            context.BindingContext.GetRequiredService<IConsoleLogger>().ExitException(exception);
            context.BindingContext.GetRequiredService<IAnsiConsole>().PrintHelp(exception);
        }
        catch (Exception ex) when (ex is OperationCanceledException or TaskCanceledException)
        {
        }
        catch (Exception exception)
        {
            context.ExitCode = ExitCodes.Error;
            App.Log.UnhandledException(exception);
            throw;
        }
    }
}

file static class LogExtensions
{
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

    public static void UnhandledException(this IConsoleLogger logger, Exception exception)
    {
        logger.Exception("Confix terminated unexpectedly.", exception);
    }
}