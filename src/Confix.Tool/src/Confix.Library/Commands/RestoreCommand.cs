using Confix.Tool.Commands.Project;

namespace Confix.Tool.Commands;

public sealed class RestoreCommand : PipelineCommand<RestoreCommandPipeline>
{
    /// <inheritdoc />
    public RestoreCommand() : base("restore")
    {
    }
}
