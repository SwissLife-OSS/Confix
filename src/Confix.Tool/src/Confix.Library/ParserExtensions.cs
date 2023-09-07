using System.CommandLine;
using System.CommandLine.Parsing;

namespace Confix.Tool;

public static class ParserExtensions
{
    public static Task<int> InvokeWithoutOutputFileAsync(
        this Parser parser,
        string[] args,
        IConsole? console = null)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] is ['@', ..])
            {
                args[i] = $"\"\\{args[i]}\"";
            }
        }

        return parser.InvokeAsync(args, console);
    }
}
