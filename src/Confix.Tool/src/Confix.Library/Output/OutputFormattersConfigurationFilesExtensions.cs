using System.CommandLine.Builder;
using Confix.Tool.Commands.Logging;

namespace Confix.Tool.Commands.Configuration;

public static class OutputFormattersConfigurationFilesExtensions
{
    public static CommandLineBuilder AddOutputFormatters(this CommandLineBuilder builder)
    {
        builder.AddOutputFormatter<ComponentListOutputFormatter>();
        builder.AddOutputFormatter<ConfigurationFeatureOutputFormatter>();
        builder.AddOutputFormatter<ConfigurationFileOutputFormatter>();
        builder.AddOutputFormatter<ReportListOutputFormatter>();
        builder.AddOutputFormatter<JsonNodeOutputFormatter>();
        return builder;
    }
}
