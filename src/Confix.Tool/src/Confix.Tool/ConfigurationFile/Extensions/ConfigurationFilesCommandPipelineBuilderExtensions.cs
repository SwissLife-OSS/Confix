using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Middlewares;

public static class ConfigurationFilesCommandPipelineBuilderExtensions
{
    public static IPipelineDescriptor UseConfigurationFiles(this IPipelineDescriptor builder)
    {
        builder.Use<ConfigurationFileMiddleware>();

        return builder;
    }
}
