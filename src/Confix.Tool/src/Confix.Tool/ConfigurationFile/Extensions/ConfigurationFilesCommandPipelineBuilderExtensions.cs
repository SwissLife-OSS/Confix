using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Middlewares;

public static class ConfigurationFilesCommandPipelineBuilderExtensions
{
    public static IPipelineDescriptor UseWriteConfigurationFiles(this IPipelineDescriptor builder)
    {
        builder.Use<WriteConfigurationFileMiddleware>();

        return builder;
    }

    public static IPipelineDescriptor UseReadConfigurationFiles(this IPipelineDescriptor builder)
    {
        builder.AddOption(OutputFileOption.Instance);
        builder.Use<ReadConfigurationFileMiddleware>();

        return builder;
    }
}
