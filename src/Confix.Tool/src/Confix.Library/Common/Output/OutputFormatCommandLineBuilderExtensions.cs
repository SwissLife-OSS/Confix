using System.CommandLine.Builder;
using Spectre.Console;

namespace Confix.Tool.Commands.Logging;

public static class OutputFormatCommandLineBuilderExtensions
{
    private static Context.Key<List<IOutputFormatter>> _key =
        new("Confix.Tool.Common.OutputFormatter");

    public static CommandLineBuilder AddOutputFormatter<T>(this CommandLineBuilder builder)
        where T : IOutputFormatter, new()
    {
        builder.GetOutputFormatters().Add(new T());
        return builder;
    }

    public static CommandLineBuilder UseOutputFormat(this CommandLineBuilder builder)
    {
        builder.AddMiddleware(async (context, next) =>
        {
            var format =
                context.ParseResult.GetValueForOption(FormatOption.Instance) ??
                context.ParseResult.GetValueForOption(FormatOptionWithDefault.Instance);

            if (format is not null)
            {
                context.SetContextData(Context.DisableStatus, true);

                // we disable logging if the format option is specified
                using (App.Log.SetVerbosity(Verbosity.Quiet))
                {
                    await next(context);
                }

                var outputFormatters = builder.GetOutputFormatters();

                if (builder.GetOutput() is { } output)
                {
                    string? formattedValue = null;

                    foreach (var outputFormatter in outputFormatters)
                    {
                        if (!outputFormatter.CanHandle(format.Value, output))
                        {
                            continue;
                        }

                        formattedValue =
                            await outputFormatter.FormatAsync(context, format.Value, output);

                        break;
                    }

                    context.Console.Out.Write(formattedValue);
                }
            }
            else
            {
                await next(context);
            }
        });

        return builder;
    }

    private static List<IOutputFormatter> GetOutputFormatters(this CommandLineBuilder builder)
        => builder.GetContextData().GetOrAddValue(_key);

    private static object? GetOutput(this CommandLineBuilder context)
    {
        return context.GetContextData().Get(Context.Output);
    }
}
