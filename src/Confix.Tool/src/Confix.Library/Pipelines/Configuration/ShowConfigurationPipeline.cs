using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;

namespace Confix.Tool.Commands.Configuration;

public sealed class ShowConfigurationPipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder
            .AddOption(FormatOptionWithDefault.Instance)
            .Use<LoadConfigurationMiddleware>()
            .UseHandler(InvokeAsync);
    }

    private static Task InvokeAsync(IMiddlewareContext context)
    {
        var configuration = context.Features.Get<ConfigurationFeature>();

        // the logging is done by the output formatter
        context.SetOutput(configuration);

        return Task.CompletedTask;
    }
}
