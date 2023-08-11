using System.CommandLine.Invocation;

namespace Confix.Tool.Commands.Logging;

public interface IOutputFormatter
{
    bool CanHandle(OutputFormat format, object value);

    Task<string> FormatAsync(InvocationContext context, OutputFormat format, object value);
}

public interface IOutputFormatter<in T> : IOutputFormatter
{
    bool IOutputFormatter.CanHandle(OutputFormat format, object value)
        => value is T t && CanHandle(format, t);

    bool CanHandle(OutputFormat format, T value);

    Task<string> IOutputFormatter.FormatAsync(
        InvocationContext context,
        OutputFormat format,
        object value)
    {
        if (value is T t)
        {
            return FormatAsync(context, format, t);
        }

        throw new ArgumentException($"Expected {typeof(T).Name} but got {value.GetType().Name}");
    }

    Task<string> FormatAsync(InvocationContext context, OutputFormat format, T value);
}
