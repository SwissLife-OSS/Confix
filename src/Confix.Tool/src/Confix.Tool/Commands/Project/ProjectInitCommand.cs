using Confix.Tool.Commands.Solution;

namespace Confix.Tool.Commands.Project;

public sealed class ProjectInitCommand : PipelineCommand<ProjectInitPipeline>
{
    /// <inheritdoc />
    public ProjectInitCommand() : base("init")
    {
        Description = "Initializes a project and creates a project file";
    }
}