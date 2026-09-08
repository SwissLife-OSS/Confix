using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Confix.Tool.Commands.Logging;

public static class OutputFormatCommandLineBuilderExtensions
{
    private static Context.Key<List<IOutputFormatter>> _key =
        new("Confix.Tool.Common.OutputFormatter");

    public static ConfixCommandLineBuilder AddOutputFormatter<T>(this ConfixCommandLineBuilder builder)
        where T : IOutputFormatter, new()
    {
        builder.GetOutputFormatters().Add(new T());
        builder.AddSingleton<IOutputFormatter>(_
            => new CombinedOutputFormatter(builder.GetOutputFormatters()));
        return builder;
    }

    public static ConfixCommandLineBuilder UseOutputFormat(this ConfixCommandLineBuilder builder)
    {
        builder.AddSingleton(new OutputFormatterCollection(builder.GetOutputFormatters()));

        return builder;
    }

    /// <summary>
    /// Executes <paramref name="action" /> and writes its output using the formatter that
    /// matches the <c>--format</c> option, if one was specified.
    /// </summary>
    public static async Task<int> ExecuteWithOutputFormatAsync(
        this IServiceProvider services,
        ParseResult parseResult,
        Func<Task<int>> action)
    {
        var format =
            parseResult.GetValue(FormatOption.Instance) ??
            parseResult.GetValue(FormatOptionWithDefault.Instance);

        if (format is null)
        {
            return await action();
        }

        services.SetContextData(Context.DisableStatus, true);

        int exitCode;

        // we disable logging if the format option is specified
        using (App.Log.SetVerbosity(Verbosity.Quiet))
        {
            exitCode = await action();
        }

        var contextData = services.GetContextData();

        if (contextData.Get(Context.Output) is { } output)
        {
            var outputFormatters =
                services.GetRequiredService<OutputFormatterCollection>().Formatters;

            string? formattedValue = null;

            foreach (var outputFormatter in outputFormatters)
            {
                if (!outputFormatter.CanHandle(format.Value, output))
                {
                    continue;
                }

                formattedValue = await outputFormatter.FormatAsync(format.Value, output);

                break;
            }

            services.GetRequiredService<IAnsiConsole>().WriteLine(formattedValue ?? string.Empty);
        }

        return exitCode;
    }

    private static List<IOutputFormatter> GetOutputFormatters(this ConfixCommandLineBuilder builder)
        => builder.GetContextData().GetOrAddValue(_key);
}

/// <summary>
/// Holds the registered output formatters so that they can be resolved from the container.
/// </summary>
public sealed class OutputFormatterCollection
{
    public OutputFormatterCollection(List<IOutputFormatter> formatters)
    {
        Formatters = formatters;
    }

    public List<IOutputFormatter> Formatters { get; }
}

file sealed class CombinedOutputFormatter : IOutputFormatter
{
    private readonly List<IOutputFormatter> _formatters;

    public CombinedOutputFormatter(List<IOutputFormatter> formatters)
    {
        _formatters = formatters;
    }

    /// <inheritdoc />
    public bool CanHandle(OutputFormat format, object value)
    {
        return _formatters.Any(f => f.CanHandle(format, value));
    }

    /// <inheritdoc />
    public Task<string> FormatAsync(OutputFormat format, object value)
    {
        return _formatters
            .First(f => f.CanHandle(format, value))
            .FormatAsync(format, value);
    }
}
