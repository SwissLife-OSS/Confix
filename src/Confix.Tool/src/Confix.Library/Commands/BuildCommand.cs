using Confix.Tool.Commands.Project;

namespace Confix.Tool.Commands;

public sealed class BuildCommand : PipelineCommand<ComponentBuildPipeline>
{
    /// <inheritdoc />
    public BuildCommand() : base("build")
    {
    }
}
