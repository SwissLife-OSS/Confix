using Confix.Tool.Abstractions;

namespace Confix.Tool.Middlewares;

public class ConfigurationFeature
{
    public ConfigurationFeature(
        ConfigurationScope scope,
        IConfigurationFileCollection configurationFiles,
        ProjectDefinition? project,
        ComponentDefinition? component,
        RepositoryDefinition? repository)
    {
        Scope = scope;
        ConfigurationFiles = configurationFiles;
        Project = project;
        Component = component;
        Repository = repository;
    }

    public ConfigurationScope Scope { get; }

    public IConfigurationFileCollection ConfigurationFiles { get; }

    public ProjectDefinition? Project { get; }

    public ComponentDefinition? Component { get; }

    public RepositoryDefinition? Repository { get; }
}