using System.CommandLine;

namespace Confix.Tool.Commands.Configuration.Arguments;

public class ConfigValueArgument : Argument<string>
{
    private const string _description = "The value to set as json";

    public ConfigValueArgument() : base("value", _description)
    {
    }

    public static readonly ConfigValueArgument Instance = new();
}
