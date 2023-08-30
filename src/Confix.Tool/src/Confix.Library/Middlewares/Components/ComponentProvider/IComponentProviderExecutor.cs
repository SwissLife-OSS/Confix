using Confix.Tool.Abstractions;
using Confix.Tool.Entities.Components;

namespace Confix.Tool.Middlewares;

public interface IComponentProviderExecutor
{
    Task ExecuteAsync(IComponentProviderContext context);

    Task<IList<Component>> LoadComponents(
        SolutionDefinition solution,
        ProjectDefinition project,
        CancellationToken cancellationToken);
}
