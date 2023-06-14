namespace Confix.Tool.Commands.Project;

public sealed class ProjectReloadCommand : PipelineCommand<ProjectReloadPipeline>
{
    public ProjectReloadCommand() : base("reload")
    {
        Description = "Reloads the schema of a project";
    }
}
