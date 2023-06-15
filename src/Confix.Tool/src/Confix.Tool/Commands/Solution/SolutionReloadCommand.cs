using Confix.Tool.Commands.Project;

namespace Confix.Tool.Commands.Solution;

public sealed class SolutionReloadCommand : PipelineCommand<SolutionReloadPipeline>
{
    /// <inheritdoc />
    public SolutionReloadCommand() : base("reload")
    {
        Description = "Reloads the schema of all the projects in the solution";
    }
}
