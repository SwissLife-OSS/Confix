using Confix.Tool.Commands.Configuration;
using Confix.Tool.Commands.Project;

namespace Confix.Tool.Commands.Config;

public sealed class ConfigListCommand : PipelineCommand<ListConfigurationPipeline>
{
    /// <inheritdoc />
    public ConfigListCommand() : base("list")
    {
        Description = "Lists the configuration to a file";
    }
}
