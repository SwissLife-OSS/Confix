using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Confix.Utilities.Json;
using Json.Schema;

namespace Confix.Tool.Abstractions;

public sealed class EnvironmentConfiguration
{
    public static class FieldNames
    {
        public const string Name = "name";
        public const string Enabled = "enabled";
        public const string Validation = "validation";
        public const string ExportSchema = "exportSchema";
    }

    public EnvironmentConfiguration(
        string? name,
        bool? enabled, ValidationConfiguration? validation = null, bool? exportSchema = null)
    {
        Name = name;
        Validation = validation;
        ExportSchema = exportSchema;
        Enabled = enabled;
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ValidationConfiguration? Validation { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ExportSchema { get; }

    public string? Name { get; }
    public bool? Enabled { get; }

    public static EnvironmentConfiguration Parse(JsonNode node)
    {
        if (node.GetValueKind() is JsonValueKind.String)
        {
            return new EnvironmentConfiguration(node.ExpectValue<string>(), null);
        }

        var obj = node.ExpectObject();
        if (obj.ContainsKey("codeFirst"))
            throw new ArgumentException("Replace codeFirst with validation: { type: \"dotnet-options\" }; move exportSchema to its parent object.");

        var name = obj.MaybeProperty(FieldNames.Name)?.ExpectValue<string>();

        var enabled = obj.MaybeProperty(FieldNames.Enabled)?.ExpectValue<bool>();

        return new EnvironmentConfiguration(name, enabled,
            ValidationConfiguration.Parse(obj.MaybeProperty(FieldNames.Validation)),
            obj.MaybeProperty(FieldNames.ExportSchema)?.ExpectValue<bool>());
    }

    public EnvironmentConfiguration Merge(EnvironmentConfiguration other)
    {
        var name = other.Name ?? Name;
        var enabled = other.Enabled ?? Enabled;

        return new EnvironmentConfiguration(name, enabled, Validation?.Merge(other.Validation) ?? other.Validation, other.ExportSchema ?? ExportSchema);
    }
}
