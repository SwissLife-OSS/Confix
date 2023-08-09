using Confix.Tool.Commands.Configuration;
using Confix.Tool.Commands.Project;

namespace Confix.Tool.Commands.Config;

public sealed class ConfigSetCommand : PipelineCommand<SetConfigurationPipeline>
{
    /// <inheritdoc />
    public ConfigSetCommand() : base("set")
    {
        Description = "Sets a configuration value in the nearest .confixrc";
    }
}
