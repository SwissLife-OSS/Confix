using System.CommandLine;

namespace Confix.Tool.Commands.Component;

public static class CommandExtensions
{
    public static CommandPipelineBuilder AddPipeline(this Command command)
    {
        return CommandPipelineBuilder.New(command);
    }
}
