using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Middlewares;

public static class CommandPipelineBuilderExtensions
{
    public static IPipelineDescriptor UseEnvironment(this IPipelineDescriptor builder)
        => builder
            .Use<EnvironmentMiddleware>()
            .AddOption(ActiveEnvironmentOption.Instance);
}
