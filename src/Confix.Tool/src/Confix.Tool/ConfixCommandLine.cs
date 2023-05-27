using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using Confix.Tool.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Confix.Tool;

internal sealed class ConfixCommandLine : CommandLineBuilder
{
    public ConfixCommandLine() : base(new ConfixRootCommand())
    {
        var console = AnsiConsole.Create(new AnsiConsoleSettings());

        console.Profile.Width = 500;

        this.AddService(console)
            .AddService<IProjectDiscovery, ProjectDiscovery>()
            .UseDefaults()
            .AddMiddleware(ExceptionMiddleware, MiddlewareOrder.ExceptionHandler);
    }

    private static async Task ExceptionMiddleware(
        InvocationContext context,
        Func<InvocationContext, Task> next)
    {
        try
        {
            await next(context);
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
