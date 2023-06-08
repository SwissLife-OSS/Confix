using Confix.Tool.Abstractions.Configuration;
using Confix.Tool.Schema;

namespace Confix.Tool.Abstractions;

public sealed class RepositoryDefinition
{
    public RepositoryDefinition(
        ProjectDefinition? project,
        ComponentDefinition? component,
        DirectoryInfo? directory)
    {
        Project = project;
        Component = component;
        Directory = directory;
    }

    public ComponentDefinition? Component { get; }

    public ProjectDefinition? Project { get; }

    public DirectoryInfo? Directory { get; }

    public static RepositoryDefinition From(RepositoryConfiguration configuration)
    {
        var lastConfigurationFile =
            configuration.SourceFiles.LastOrDefault(x => x.Name == FileNames.ConfixRepository);

        var project = configuration.Project is not null
            ? ProjectDefinition.From(configuration.Project)
            : null;

        var component = configuration.Component is not null
            ? ComponentDefinition.From(configuration.Component)
            : null;

        return new RepositoryDefinition(project, component, lastConfigurationFile?.Directory);
    }
}
