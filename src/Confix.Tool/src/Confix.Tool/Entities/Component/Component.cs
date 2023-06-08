using Json.Schema;

namespace Confix.Tool.Abstractions;

public class Component
{
    public Component(
        string provider,
        string componentName,
        string? version,
        bool isEnabled,
        IReadOnlyList<string> mountingPoints,
        JsonSchema schema)
    {
        Provider = provider;
        ComponentName = componentName;
        Version = version;
        IsEnabled = isEnabled;
        MountingPoints = mountingPoints;
        Schema = schema;
    }

    public string Provider { get; }

    public string ComponentName { get; }

    public string? Version { get; }

    public bool IsEnabled { get; }

    public JsonSchema Schema { get; }

    public IReadOnlyList<string> MountingPoints { get; }
}
