using System.Text.Json;
using Confix.Extensions;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Schema;
using Confix.Utilities.Json;

namespace Confix.Tool.Middlewares.Artifact;

public sealed class WriteArtifactMiddleware : IMiddleware
{
    /// <inheritdoc />
    public async Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        var configuration = context.Features.Get<ConfigurationFeature>();

        if (context.Parameter.TryGet(ArtifactFileOption.Instance, out FileInfo file) &&
            configuration.Project?.Directory is { } projectDirectory)
        {
            var serialized = JsonSerializer.SerializeToNode(
                configuration,
                JsonSerialization.Instance.ConfigurationFeature);

            var rewritten = ArtifactFileRewriter.Instance
                .Rewrite(serialized!, new ArtifactFileContext(projectDirectory));

            await using var stream = file.OpenReplacementStream();

            await rewritten.SerializeToStreamAsync(stream, context.CancellationToken);
        }

        await next(context);
    }
}
