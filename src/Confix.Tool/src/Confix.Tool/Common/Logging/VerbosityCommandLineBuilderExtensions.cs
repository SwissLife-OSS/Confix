using System.CommandLine.Builder;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Confix.Tool.Commands.Logging;

public static class VerbosityCommandLineBuilderExtensions
{
    public static CommandLineBuilder UseVerbosity(this CommandLineBuilder builder)
    {
        builder.AddMiddleware((context, next) =>
        {
            var verbosity =
                context.ParseResult.GetValueForOption(VerbosityOption.Instance);

            var console = context.BindingContext.GetRequiredService<IAnsiConsole>();

            var logger = new ConsoleLogger(console, verbosity);

            context.BindingContext.AddService(typeof(IConsoleLogger), _ => logger);

            App.Log = logger;

            return next(context);
        });

        return builder;
    }
}
