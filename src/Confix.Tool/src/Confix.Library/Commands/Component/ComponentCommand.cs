using System.CommandLine;
using Confix.Commands.Component;
using Confix.Tool.Commands.Project;

namespace Confix.Tool.Commands.Component;

public sealed class ComponentCommand : Command
{
    public ComponentCommand() : base("component")
    {
        Description = "This command is used to manage components.";
        AddCommand(new BuildComponentCommand());
        AddCommand(new ComponentInitCommand());
        AddCommand(new ComponentListCommand());
    }
}
