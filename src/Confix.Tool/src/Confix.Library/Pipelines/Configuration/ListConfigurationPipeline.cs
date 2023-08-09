using System.Buffers;
using System.Text;
using System.Text.Json;
using Confix.Extensions;
using Confix.Tool.Commands.Configuration.Arguments;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Spectre.Console;

namespace Confix.Tool.Commands.Configuration;

public sealed class ListConfigurationPipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder
            .Use<LoadConfigurationMiddleware>()
            .UseHandler(InvokeAsync);
    }

    private static Task InvokeAsync(IMiddlewareContext context)
    {
        var configuration = context.Features.Get<ConfigurationFeature>();

        foreach (var file in configuration.ConfigurationFiles)
        {
            context.Logger.Information(" - " + file.File.FullName);
        }

        return Task.CompletedTask;
    }
}
