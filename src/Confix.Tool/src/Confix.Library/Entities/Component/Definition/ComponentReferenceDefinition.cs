using System.Text.Json;

namespace Confix.Tool.Abstractions;

public sealed class ComponentReferenceDefinition
{
    public ComponentReferenceDefinition(
        string provider,
        string componentName,
        string? version,
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

    public string? Version { get; }

    public bool IsEnabled { get; }

    public IReadOnlyList<string> MountingPoints { get; }

    public string GetKey() => $"@{Provider}/{ComponentName}";

    public void WriteTo(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();

        writer.WriteString("version", Version);

        writer.WriteStartArray("mountingPoint");

        foreach (var mountingPoint in MountingPoints)
        {
            writer.WriteStringValue(mountingPoint);
        }

        writer.WriteEndArray();

        writer.WriteEndObject();
    }

    public static ComponentReferenceDefinition From(ComponentReferenceConfiguration configuration)
    {
        List<string> validationErrors = new();
        if (configuration.Provider is null)
        {
            validationErrors.Add("Provider is not defined.");
        }

        if (configuration.ComponentName is null)
        {
            validationErrors.Add("Component name is not defined.");
        }

        if (validationErrors.Any())
        {
            throw new ValidationException("Invalid component reference configuration")
            {
                Errors = validationErrors
            };
        }

        var mountingPoints = configuration.MountingPoints ?? Array.Empty<string>();
        if (mountingPoints.Count == 0)
        {
            mountingPoints = new[] { configuration.ComponentName! };
        }

        return new ComponentReferenceDefinition(
            configuration.Provider!,
            configuration.ComponentName!,
            configuration.Version,
            configuration.IsEnabled,
            mountingPoints);
    }
}
