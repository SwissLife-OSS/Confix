using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using Microsoft.Extensions.DependencyInjection;
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

            var console = context.BindingContext.GetRequiredService<IAnsiConsole>();
            foreach (var exitException in exception.InnerExceptions)
            {
                console.Error(exitException.Message);
            }
        }
        catch (ExitException exception) when (exception is { Message: var message })
        {
            context.ExitCode = ExitCodes.Error;

            context.BindingContext.GetRequiredService<IAnsiConsole>().Error(message);
        }
        catch (Exception ex) when (ex is OperationCanceledException or TaskCanceledException)
        {
        }
        catch (Exception exception)
        {
            context.ExitCode = ExitCodes.Error;
            context.BindingContext.GetRequiredService<IAnsiConsole>().WriteException(exception);
            throw;
        }
    }
}
