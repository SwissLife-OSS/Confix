using System.CommandLine;

namespace Confix.Tool.Commands.Configuration.Arguments;

public class ConfigPathArgument : Argument<string>
{
    private const string _description = "The path to the configuration file";

    public ConfigPathArgument() : base("path", _description)
    {
    }

    public static readonly ConfigPathArgument Instance = new();
}
