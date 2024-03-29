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

    public string? Version { get; set; }

    public bool IsEnabled { get; set; }

    public JsonSchema Schema { get; set; }

    public IReadOnlyList<string> MountingPoints { get; set; }
    
    public string GetKey()
        => ComponentHelpers.GetKey(Provider, ComponentName);
}
