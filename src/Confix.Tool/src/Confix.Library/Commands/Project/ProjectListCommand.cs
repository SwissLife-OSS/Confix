namespace Confix.Tool.Commands.Project;

public sealed class ProjectListCommand : PipelineCommand<ProjectListPipeline>
{
    public ProjectListCommand() : base("list")
    {
        Description = "Lists all .confix.project files in the current directory and subdirectories";
    }
}
