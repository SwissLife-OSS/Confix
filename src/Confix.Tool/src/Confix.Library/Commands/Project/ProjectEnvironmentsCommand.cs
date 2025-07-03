using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Commands.Project;

public sealed class ProjectEnvironmentsCommand : PipelineCommand<ProjectEnvironmentsPipeline>
{
    public ProjectEnvironmentsCommand() : base("environments")
    {
        Description = "Lists all environments defined in the current project";
    }
}
