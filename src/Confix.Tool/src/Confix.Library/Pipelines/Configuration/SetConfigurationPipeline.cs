using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Tool.Commands.Configuration.Arguments;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Schema;
using Confix.Utilities.Json;

namespace Confix.Tool.Commands.Configuration;

public sealed class SetConfigurationPipeline : Pipeline
{
    /// <inheritdoc />
    protected override void Configure(IPipelineDescriptor builder)
    {
        builder
            .AddArgument(ConfigPathArgument.Instance)
            .AddArgument(ConfigValueArgument.Instance)
            .Use<LoadConfigurationMiddleware>()
            .UseHandler(InvokeAsync);
    }

    private static Task InvokeAsync(IMiddlewareContext context)
    {
        var configuration = context.Features.Get<ConfigurationFeature>();

        var closestConfixRc =
            configuration.ConfigurationFiles.LastOrDefault(x => x.File.Name == FileNames.ConfixRc);

        if (closestConfixRc == null)
        {
            throw new ExitException("Could not find a confix.rc.json file");
        }

        var path = context.Parameter.Get(ConfigPathArgument.Instance);
        var value = context.Parameter.Get(ConfigValueArgument.Instance);

        if (!value.TryParse(out var node))
        {
            throw new ExitException("Could not parse the value as json");
        }

        closestConfixRc.Content.SetValue(path, node);

        File.WriteAllText(closestConfixRc.File.FullName, closestConfixRc.Content.ToJsonString());

        return Task.CompletedTask;
    }
}

file static class Extensions
{
    public static bool TryParse(this string value, [NotNullWhen(true)] out JsonNode? node)
    {
        try
        {
            node = JsonNode.Parse(value);
            return node is not null;
        }
        catch (JsonException)
        {
            node = null;
            return false;
        }
    }
}
