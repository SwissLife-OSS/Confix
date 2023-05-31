using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Middlewares;

public sealed class LoadConfiguration : IMiddleware
{
    /// <inheritdoc />
    public Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        context.Features.Set(new ConfigurationFeature());
        throw new NotImplementedException();
    }
}

public class ConfigurationFeature
{
    public ConfigurationScope Scope { get; set; }
}

public enum ConfigurationScope
{
    Component,
    Project,
    Repository
}

