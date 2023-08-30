using Confix.Tool.Commands.Project;

namespace Confix.Tool.Commands.Solution;

public sealed class SolutionRestoreCommand : PipelineCommand<SolutionRestorePipeline>
{
    /// <inheritdoc />
    public SolutionRestoreCommand() : base("restore")
    {
        Description = "Reloads the schema of all the projects in the solution";
    }
}
