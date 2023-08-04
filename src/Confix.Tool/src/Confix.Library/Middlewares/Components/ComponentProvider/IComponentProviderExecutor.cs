using Confix.Tool.Entities.Components;

namespace Confix.Tool.Middlewares;

public interface IComponentProviderExecutor
{
    Task ExecuteAsync(IComponentProviderContext context);
}
