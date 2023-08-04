using Confix.Tool.Commands.Project;

namespace Confix.Tool.Commands.Solution;

public sealed class SolutionBuildCommand : PipelineCommand<SolutionBuildPipeline>
{
    /// <inheritdoc />
    public SolutionBuildCommand() : base("build")
    {
        Description = "Replaces all variables in the solution files with their values";
    }
}
