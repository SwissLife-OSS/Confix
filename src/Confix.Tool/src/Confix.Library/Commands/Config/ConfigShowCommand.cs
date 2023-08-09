using Confix.Tool.Commands.Configuration;
using Confix.Tool.Commands.Project;

namespace Confix.Tool.Commands.Config;

public sealed class ConfigShowCommand : PipelineCommand<ShowConfigurationPipeline>
{
    /// <inheritdoc />
    public ConfigShowCommand() : base("show")
    {
        Description = "Shows the configuration to a file";
    }
}