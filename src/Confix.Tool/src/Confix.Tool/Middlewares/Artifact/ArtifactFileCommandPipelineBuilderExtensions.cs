using System.CommandLine.Builder;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Entities.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool.Middlewares.Artifact;

public static class ArtifactFileCommandPipelineBuilderExtensions
{
    public static IPipelineDescriptor UseWriteArtifactFile(this IPipelineDescriptor builder)
    {
        builder.AddOption(ArtifactFileOption.Instance);
        builder.Use<WriteArtifactMiddleware>();

        return builder;
    }

    public static IPipelineDescriptor UseReadArtifactFile(this IPipelineDescriptor builder)
    {
        builder.AddOption(ArtifactFileOption.Instance);
        builder.Use<ReadArtifactMiddleware>();

        return builder;
    }

    public static CommandLineBuilder RegisterArtifactFile(this CommandLineBuilder builder)
    {
        builder.AddSingleton<ReadArtifactMiddleware>();
        builder.AddSingleton<WriteArtifactMiddleware>();

        return builder;
    }
}
