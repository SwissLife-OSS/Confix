using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Middlewares.Project;

public static class BuildComponentsOfProjectExtensions
{
    public static IPipelineDescriptor UseBuildComponentsOfProject(
        this IPipelineDescriptor descriptor)
    {
        descriptor.Use<BuildComponentsOfProjectMiddleware>();
        descriptor.Add(OnlyComponentsOption.Instance);
        return descriptor;
    }
}
