using System.CommandLine;

namespace Confix.Tool.Common.Pipelines;

public static class CommandExtensions
{
    public static CommandPipelineBuilder AddPipeline(this Command command)
    {
        return CommandPipelineBuilder.New(command);
    }
}
