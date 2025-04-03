using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Entities.Components;

public class ComponentProviderContext
    : IComponentProviderContext
{
    public ComponentProviderContext(
        IConsoleLogger logger,
        CancellationToken cancellationToken,
        ProjectDefinition project,
        SolutionDefinition solution,
        IParameterCollection parameter,
        IReadOnlyList<ComponentReferenceDefinition> componentReferences)
    {
        Logger = logger;
        CancellationToken = cancellationToken;
        Project = project;
        Solution = solution;
        Parameter = parameter;
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

    public IParameterCollection Parameter { get; }
}
