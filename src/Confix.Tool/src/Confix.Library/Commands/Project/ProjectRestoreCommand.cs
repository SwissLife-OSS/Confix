namespace Confix.Tool.Commands.Project;

public sealed class ProjectRestoreCommand : PipelineCommand<ProjectRestorePipeline>
{
    public ProjectRestoreCommand() : base("restore")
    {
        Description = "Reloads the schema of a project";
    }
}
