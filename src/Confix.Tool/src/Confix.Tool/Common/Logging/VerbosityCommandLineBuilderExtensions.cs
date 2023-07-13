using System.CommandLine.Builder;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Confix.Tool.Commands.Logging;

public static class VerbosityCommandLineBuilderExtensions
{
    public static CommandLineBuilder UseVerbosity(this CommandLineBuilder builder)
    {
        var verbosity = Verbosity.Normal;
        builder.AddSingleton<IConsoleLogger>(sp =>
        {
            var console = sp.GetRequiredService<IAnsiConsole>();

            return new ConsoleLogger(console, verbosity);
        });
        builder.AddMiddleware((context, next) =>
        {
            verbosity =
                context.ParseResult.GetValueForOption(VerbosityOption.Instance);

            App.Log = context.BindingContext.GetRequiredService<IConsoleLogger>();

            return next(context);
        });

        return builder;
    }
}
