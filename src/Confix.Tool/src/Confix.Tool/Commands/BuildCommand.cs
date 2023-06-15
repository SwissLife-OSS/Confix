using Confix.Tool.Commands.Project;

namespace Confix.Tool.Commands;

public sealed class BuildCommand : PipelineCommand<BuildCommandPipeline>
{
    /// <inheritdoc />
    public BuildCommand() : base("build")
    {
    }
}
