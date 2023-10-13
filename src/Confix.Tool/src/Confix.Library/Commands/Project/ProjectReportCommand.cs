namespace Confix.Tool.Commands.Project;

public sealed class ProjectReportCommand : PipelineCommand<ProjectReportPipeline>
{
    public ProjectReportCommand() : base("build")
    {
        Description = "Replaces all variables in the project files with their values";
    }
}
