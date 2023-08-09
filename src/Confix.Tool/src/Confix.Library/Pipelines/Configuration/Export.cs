using System.Buffers;
using System.Text;
using System.Text.Json;
using Confix.Extensions;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Spectre.Console;

namespace Confix.Tool.Commands.Configuration;

public sealed class ShowConfigurationPipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder
            .AddContextData(Context.DisableStatus, true)
            .UseNoLog()
            .Use<LoadConfigurationMiddleware>()
            .UseHandler(InvokeAsync);
    }

    private static Task InvokeAsync(IMiddlewareContext context)
    {
        var configuration = context.Features.Get<ConfigurationFeature>();

        var json = configuration.ToJsonString();

        context.Console.WriteLine(json);

        return Task.CompletedTask;
    }
}

file static class Extensions
{
    public static string ToJsonString(this ConfigurationFeature definition)
    {
        var options = new JsonWriterOptions
        {
            Indented = true
        };

        var buffer = new ArrayBufferWriter<byte>();

        using var writer = new Utf8JsonWriter(buffer, options);
        definition.WriteTo(writer);
        writer.Flush();

        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    public static IPipelineDescriptor UseNoLog(this IPipelineDescriptor builder)
    {
        builder.Use(async (ctx, next) =>
        {
            using var _ = ctx.Logger.SetVerbosity(Verbosity.Quiet);
            await next(ctx);
        });
        return builder;
    }
}
