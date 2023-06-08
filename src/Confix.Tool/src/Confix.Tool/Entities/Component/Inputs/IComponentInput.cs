using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Entities.Components;

public interface IComponentInput
{
    public static virtual string Type => string.Empty;

    Task ExecuteAsync(IMiddlewareContext context);
}
