using Confix.Tool.Commands.Project;

namespace Confix.Tool.Commands.Solution;

public sealed class SolutionValidateCommand : PipelineCommand<SolutionValidatePipeline>
{
    /// <inheritdoc />
    public SolutionValidateCommand() : base("validate")
    {
        Description = "Validates the schema of all the projects in the solution";
    }
}
