using System.Text.Json;
using Confix.Extensions;
using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Middlewares.Artifact;

public sealed class ReadArtifactMiddleware : IMiddleware
{
    /// <inheritdoc />
    public async Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        if (context.Parameter.TryGet(ArtifactFileOption.Instance, out FileInfo file))
        {
            await using var stream = file.OpenRead();

            var configurationFeature = await JsonSerializer.DeserializeAsync(
                stream,
                JsonSerialization.Instance.ConfigurationFeature,
                context.CancellationToken);

            context.Features.Set(configurationFeature);
        }

        await next(context);
    }
}
