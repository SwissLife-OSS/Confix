using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Middlewares.Project;
using Confix.Tool.Validation;

namespace Confix.Tool.Commands.Project;

public sealed class ProjectValidatePipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder
            .Add(ConfigurationValidationPipeline.ExportSchemaOption)
            .Add(DotnetConfigurationOptions.Instance)
            .Add(GitUsernameOptions.Instance)
            .Add(GitTokenOptions.Instance)
            .Use<LoadConfigurationMiddleware>()
            .Use((context, next) => ConfigurationValidationPipeline.DispatchAsync(context, next, false))
            .UseReadConfigurationFiles()
            .UseEnvironment()
            .UseBuildComponentsOfProject()
            .UseCompleteWhenNoConfigurationFiles()
            .Use<VariableMiddleware>()
            .Use<BuildProjectMiddleware>()
            .Use<ValidationMiddleware>();
    }
}
