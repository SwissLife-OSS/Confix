using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Middlewares;

public static class ConfigurationFilesCommandPipelineBuilderExtensions
{
    public static CommandPipelineBuilder UseConfigurationFiles(this CommandPipelineBuilder builder)
    {
        builder.Use<ConfigurationFileMiddleware>();

        return builder;
    }
}
