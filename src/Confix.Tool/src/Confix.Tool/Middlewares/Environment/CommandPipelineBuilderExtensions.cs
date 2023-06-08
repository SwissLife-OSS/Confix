using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Middlewares;

public static class CommandPipelineBuilderExtensions
{
    public static CommandPipelineBuilder UseEnvironment(this CommandPipelineBuilder builder) 
        => builder
            .Use<EnvironmentMiddleware>()
            .AddOption(ActiveEnvironmentOption.Instance);
}
