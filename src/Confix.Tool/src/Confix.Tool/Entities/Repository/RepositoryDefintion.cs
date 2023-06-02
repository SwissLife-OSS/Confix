using Confix.Tool.Abstractions.Configuration;

namespace Confix.Tool.Abstractions;

public sealed class RepositoryDefinition
{
    public RepositoryDefinition(
        ProjectDefinition? project,
        ComponentDefinition? component)
    {
        Project = project;
        Component = component;
    }

    public ComponentDefinition? Component { get; }

    public ProjectDefinition? Project { get; }

    public static RepositoryDefinition From(RepositoryConfiguration configuration)
    {
        var project = configuration.Project is not null
            ? ProjectDefinition.From(configuration.Project)
            : null;

        var component = configuration.Component is not null
            ? ComponentDefinition.From(configuration.Component)
            : null;

        return new RepositoryDefinition(project, component);
    }
}
