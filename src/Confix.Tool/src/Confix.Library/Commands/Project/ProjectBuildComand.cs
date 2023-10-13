namespace Confix.Tool.Commands.Project;

public sealed class ProjectBuildCommand : PipelineCommand<ProjectBuildPipeline>
{
    public ProjectBuildCommand() : base("build")
    {
        Description = "Replaces all variables in the project files with their values";
    }
}