using System.CommandLine;
using Confix.Commands.Component;
using Confix.Tool.Commands.Project;

namespace Confix.Tool.Commands.Component;

public sealed class ComponentCommand : Command
{
    public ComponentCommand() : base("component")
    {
        Description = "This command is used to manage components.";
        Add(new BuildComponentCommand());
        Add(new ComponentInitCommand());
        Add(new ComponentListCommand());
        Add(new ComponentAddCommand());
    }
}
