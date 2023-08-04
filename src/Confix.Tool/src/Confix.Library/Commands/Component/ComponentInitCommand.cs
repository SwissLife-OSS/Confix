using Confix.Tool.Commands.Solution;

namespace Confix.Tool.Commands.Project;

public sealed class ComponentInitCommand : PipelineCommand<ComponentInitPipeline>
{
    /// <inheritdoc />
    public ComponentInitCommand() : base("init")
    {
        Description = "Initializes a component and creates a component file";
    }
}
