using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Confix.Tool.Commands.Logging;

public static class VerbosityCommandLineBuilderExtensions
{
    public static ConfixCommandLineBuilder UseVerbosity(this ConfixCommandLineBuilder builder)
    {
        builder.AddSingleton(new VerbosityHolder());
        builder.AddSingleton<IConsoleLogger>(sp =>
        {
            var console = sp.GetRequiredService<IAnsiConsole>();

            return new ConsoleLogger(console, sp.GetRequiredService<VerbosityHolder>().Verbosity);
        });

        return builder;
    }

    /// <summary>
    /// Applies the verbosity that was specified on the command line and sets up the global logger.
    /// </summary>
    public static void ApplyVerbosity(this IServiceProvider services, ParseResult parseResult)
    {
        services.GetRequiredService<VerbosityHolder>().Verbosity =
            parseResult.GetValue(VerbosityOption.Instance);

        App.Log = services.GetRequiredService<IConsoleLogger>();
    }
}

/// <summary>
/// Carries the verbosity from the parsed command line to the logger factory.
/// </summary>
public sealed class VerbosityHolder
{
    public Verbosity Verbosity { get; set; } = Verbosity.Normal;
}
