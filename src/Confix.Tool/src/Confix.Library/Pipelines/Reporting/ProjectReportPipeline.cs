using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Middlewares.Project;

namespace Confix.Tool.Reporting;

public sealed class ProjectReportPipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder
            .AddOption(NoRestoreOptions.Instance)
            .AddOption(ActiveEnvironmentOption.Instance)
            .AddOption(ReportOutputFileOption.Instance)
            .Use<LoadConfigurationMiddleware>()
            .AddContextData(Context.DisableConfigurationWrite, true)
            .Use<ReadConfigurationFileMiddleware>()
            .UseEnvironment()
            .UseBuildComponentsOfProject()
            .UseCompleteWhenNoConfigurationFiles()
            .Use<VariableMiddleware>()
            .Use<ProjectReportMiddleware>();
    }
}
