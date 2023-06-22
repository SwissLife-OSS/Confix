using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using Confix.Tool.Commands.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

            var console = context.BindingContext.GetRequiredService<IConsoleLogger>();
            foreach (var innerException in exception.InnerExceptions)
            {
                if (innerException is ExitException exitException)
                {
                    console.ExitException(exitException);
                }
                else
                {
                    console.UnhandledException(innerException);
                }
            }
        }
        catch (ExitException exception) when (exception is { Message: var message })
        {
            context.ExitCode = ExitCodes.Error;

            context.BindingContext.GetRequiredService<IConsoleLogger>().ExitException(exception);
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
        if (exception.Help is not null)
        {
            ILoggerMessage helpMessage = new DefaultLoggerMessage
            {
                Verbosity = Verbosity.Normal,
                Template = $"[green1]{exception.Help}[/]",
                Glyph = Glyph.LightBulb
            };
            logger.Log(ref helpMessage);
        }
        logger.TraceException(exception);
    }

    public static void UnhandledException(this IConsoleLogger logger, Exception exception)
    {
        logger.Exception("Confix terminated unexpectedly.", exception);
    }
}