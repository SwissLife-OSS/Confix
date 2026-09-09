using System.CommandLine;

namespace Confix.Tool.Commands.Config;

public sealed class ConfigCommand : Command
{
    public ConfigCommand() : base("config")
    {
        Description = "This command is used to manage config.";
        Add(new ConfigShowCommand());
        Add(new ConfigSetCommand());
        Add(new ConfigListCommand());
    }
}