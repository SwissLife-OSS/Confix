using Confix.Tool.Commands.Project;

namespace Confix.Commands.Component;

public sealed class ComponentAddCommand : PipelineCommand<AddComponentPipeline>
{
    /// <inheritdoc />
    public ComponentAddCommand() : base("add")
    {
        Description = "Adds a component to the project";
    }
}
