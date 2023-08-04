using Confix.Tool.Commands.Project;

namespace Confix.Tool.Commands;

public sealed class ReloadCommand : PipelineCommand<ReloadCommandPipeline>
{
    /// <inheritdoc />
    public ReloadCommand() : base("reload")
    {
    }
}
