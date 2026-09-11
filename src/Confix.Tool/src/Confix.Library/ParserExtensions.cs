using System.CommandLine;

namespace Confix.Tool;

public static class ParserExtensions
{
    public static Task<int> InvokeWithoutOutputFileAsync(
        this ConfixCommandLineBuilder builder,
        string[] args,
        InvocationConfiguration? configuration = null,
        CancellationToken cancellationToken = default)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] is ['@', ..])
            {
                args[i] = $"\"\\{args[i]}\"";
            }
        }

        // the service provider has to exist before the action of a command is executed
        builder.BuildServices();

        return builder.Command
            .Parse(args)
            .InvokeAsync(configuration ?? new InvocationConfiguration(), cancellationToken);
    }
}
