using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;

namespace Confix.Tool.Entities.Components;

public class ComponentProviderContext
    : IComponentProviderContext
{
    public ComponentProviderContext(
        IConsoleLogger logger,
        CancellationToken cancellationToken,
        ProjectDefinition project,
        SolutionDefinition solution,
        IReadOnlyList<ComponentReferenceDefinition> componentReferences)
    {
        Logger = logger;
        CancellationToken = cancellationToken;
        Project = project;
        Solution = solution;
        ComponentReferences = componentReferences;
    }

    /// <inheritdoc />
    public ProjectDefinition Project { get; }

    /// <inheritdoc />
    public SolutionDefinition Solution { get; }

    public IReadOnlyList<ComponentReferenceDefinition> ComponentReferences { get; }

    /// <inheritdoc />
    public IConsoleLogger Logger { get; }

    /// <inheritdoc />
    public CancellationToken CancellationToken { get; }

    /// <inheritdoc />
    public IList<Component> Components { get; } = new List<Component>();
}
