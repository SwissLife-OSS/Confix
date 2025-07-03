namespace Confix.Tool.Commands.Project;

public sealed class ListProjectsCommand : PipelineCommand<ListProjectsPipeline>
{
    public ListProjectsCommand() : base("projects")
    {
        Description = "Lists all .confix.project files in the current directory and subdirectories";
    }
}
