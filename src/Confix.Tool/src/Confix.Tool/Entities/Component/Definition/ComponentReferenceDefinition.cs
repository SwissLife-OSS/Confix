namespace Confix.Tool.Abstractions;

public sealed class ComponentReferenceDefinition
{
    public ComponentReferenceDefinition(
        string provider,
        string componentName,
        string version,
        bool isEnabled,
        IReadOnlyList<string> mountingPoints)
    {
        Provider = provider;
        ComponentName = componentName;
        Version = version;
        IsEnabled = isEnabled;
        MountingPoints = mountingPoints;
    }

    public string Provider { get; }

    public string ComponentName { get; }

    public string Version { get; }

    public bool IsEnabled { get; }

    public IReadOnlyList<string> MountingPoints { get; }

    public static ComponentReferenceDefinition From(ComponentReferenceConfiguration configuration)
    {
        var provider = configuration.Provider ??
            throw new InvalidOperationException("Component provider is required.");

        var componentName = configuration.ComponentName ??
            throw new InvalidOperationException("Component name is required.");

        var version = configuration.Version ??
            throw new InvalidOperationException("Component version is required.");

        var mountingPoints = configuration.MountingPoints ?? Array.Empty<string>();

        return new ComponentReferenceDefinition(
            provider,
            componentName,
            version,
            configuration.IsEnabled,
            mountingPoints);
    }
}