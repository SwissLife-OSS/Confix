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
        RepositoryDefinition repository)
    {
        Logger = logger;
        CancellationToken = cancellationToken;
        Project = project;
        Repository = repository;
    }

    /// <inheritdoc />
    public ProjectDefinition Project { get; }

    /// <inheritdoc />
    public RepositoryDefinition Repository { get; }

    /// <inheritdoc />
    public IConsoleLogger Logger { get; }

    /// <inheritdoc />
    public CancellationToken CancellationToken { get; }

    /// <inheritdoc />
    public IList<Component> Components { get; } = new List<Component>();
}
