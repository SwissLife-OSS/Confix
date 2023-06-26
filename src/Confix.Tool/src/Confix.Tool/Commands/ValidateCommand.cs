using Confix.Tool.Commands.Project;

namespace Confix.Tool.Commands;

public sealed class ValidateCommand : PipelineCommand<ValidateCommandPipeline>
{
    /// <inheritdoc />
    public ValidateCommand() : base("validate")
    {
        Description = "Validates the schema of all the projects";
    }
}
