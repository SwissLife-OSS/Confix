using Confix.Tool.Commands.Project;

namespace Confix.Tool.Commands.Solution;

public class SolutionInitCommand : PipelineCommand<SolutionInitPipeline>
{
    public SolutionInitCommand() : base("init")
    {
        Description = "Initializes a solution and creates a solution file";
    }
}

