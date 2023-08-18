using Confix.Tool.Commands.Component;
using Confix.Tool.Commands.Project;
using Confix.Tool.Commands.Solution;

namespace Confix.Commands.Component;

public sealed class ComponentListCommand : PipelineCommand<ListComponentPipeline>
{
    /// <inheritdoc />
    public ComponentListCommand() : base("list")
    {
        Description = "Lists the component of the project";
    }
}
