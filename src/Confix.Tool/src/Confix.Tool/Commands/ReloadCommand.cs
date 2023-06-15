using Confix.Tool.Commands.Project;

namespace Confix.Tool.Commands;

public class ReloadCommand : PipelineCommand<ReloadCommandPipeline>
{
    /// <inheritdoc />
    public ReloadCommand() : base("reload")
    {
    }
}
