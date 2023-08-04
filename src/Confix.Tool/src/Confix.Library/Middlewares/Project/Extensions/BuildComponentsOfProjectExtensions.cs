using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Middlewares.Project;

public static class BuildComponentsOfProjectExtensions
{
    public static IPipelineDescriptor UseBuildComponentsOfProject(
        this IPipelineDescriptor descriptor)
    {
        descriptor.Use<BuildComponentsOfProjectMiddleware>();
        descriptor.AddOption(OnlyComponentsOption.Instance);
        return descriptor;
    }
}
