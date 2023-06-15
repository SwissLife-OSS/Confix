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
        SolutionDefinition solution)
    {
        Logger = logger;
        CancellationToken = cancellationToken;
        Project = project;
        Solution = solution;
    }

    /// <inheritdoc />
    public ProjectDefinition Project { get; }

    /// <inheritdoc />
    public SolutionDefinition Solution { get; }

    /// <inheritdoc />
    public IConsoleLogger Logger { get; }

    /// <inheritdoc />
    public CancellationToken CancellationToken { get; }

    /// <inheritdoc />
    public IList<Component> Components { get; } = new List<Component>();
}
