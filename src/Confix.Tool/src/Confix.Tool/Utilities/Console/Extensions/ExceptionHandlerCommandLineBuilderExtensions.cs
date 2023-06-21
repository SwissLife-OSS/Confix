using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using Confix.Tool.Commands.Logging;
using Microsoft.Extensions.DependencyInjection;

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
            foreach (var exitException in exception.InnerExceptions)
            {
                console.ExitException(exitException);
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
    public static void ExitException(this IConsoleLogger logger, Exception exception)
    {
        logger.Error("Confix failed.");
        logger.Information($"[red]{exception.Message}[/]");
        if (exception.InnerException is not null)
        {
            logger.Debug(exception.InnerException.Message);
        }
        logger.TraceException(exception);
    }

    public static void UnhandledException(this IConsoleLogger logger, Exception exception)
    {
        logger.Exception("Confix terminated unexpectedly.", exception);
    }
}