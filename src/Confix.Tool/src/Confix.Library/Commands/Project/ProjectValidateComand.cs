namespace Confix.Tool.Commands.Project;

public sealed class ProjectValidateCommand : PipelineCommand<ProjectValidatePipeline>
{
    public ProjectValidateCommand() : base("validate")
    {
        Description = "Validates the configuration files of a project";
    }
}
