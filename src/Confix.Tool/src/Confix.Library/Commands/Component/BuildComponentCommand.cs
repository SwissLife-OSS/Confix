using Confix.Tool.Commands.Project;

namespace Confix.Tool.Commands.Component;

public sealed class BuildComponentCommand : PipelineCommand<ComponentBuildPipeline>
{
    public BuildComponentCommand() : base("build")
    {
        Description = "builds a component. Runs all configured component inputs";
    }
}
