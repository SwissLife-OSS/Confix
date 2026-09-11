using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Confix.Utilities.Json;

namespace Confix.Tool.Abstractions;

/// <summary>Selects configuration validation independently of component input formats.</summary>
public sealed record ValidationConfiguration(
    string? Type = null,
    string? Coverage = null,
    string? Framework = null,
    string[]? Overlays = null)
{
    public static class FieldNames
    {
        public const string Type = "type";
        public const string Coverage = "coverage";
        public const string Framework = "framework";
        public const string Overlays = "overlays";
    }

    internal static JsonSerializerOptions SerializerOptions { get; } =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    [JsonIgnore]
    public string EffectiveType => Type ?? "json-schema";

    public ValidationConfiguration Merge(ValidationConfiguration? other)
    {
        if (other is null)
        {
            return this;
        }
        // Provider-specific settings must not leak when switching providers.
        if (other.Type is not null && Type is not null && other.Type != Type)
        {
            return other;
        }
        return new(other.Type ?? Type, other.Coverage ?? Coverage,
            other.Framework ?? Framework, other.Overlays ?? Overlays);
    }

    public static ValidationConfiguration? Parse(JsonNode? node)
    {
        if (node is null)
        {
            return null;
        }
        if (node is not JsonObject obj)
        {
            throw new ArgumentException("project.validation must be an object.");
        }
        if (obj.Any(p => p.Key is not (FieldNames.Type or FieldNames.Coverage
            or FieldNames.Framework or FieldNames.Overlays)))
        {
            throw new ArgumentException("Unknown project.validation setting.");
        }
        var result = new ValidationConfiguration(
            obj.MaybeProperty(FieldNames.Type)?.ExpectValue<string>(),
            obj.MaybeProperty(FieldNames.Coverage)?.ExpectValue<string>(),
            obj.MaybeProperty(FieldNames.Framework)?.ExpectValue<string>(),
            obj.MaybeProperty(FieldNames.Overlays)?.ExpectArray()
                .Select(overlay => overlay.ExpectValue<string>()).ToArray());
        if (result.Type is not (null or "json-schema" or "dotnet-options"))
        {
            throw new ArgumentException("project.validation.type must be json-schema or dotnet-options.");
        }
        if (result.Coverage is not (null or "strict" or "registeredSections"))
        {
            throw new ArgumentException("project.validation.coverage must be strict or registeredSections.");
        }
        if (result.Overlays?.Any(string.IsNullOrWhiteSpace) == true)
        {
            throw new ArgumentException("project.validation.overlays must not contain empty paths.");
        }
        if (result.Type == "json-schema" && (result.Coverage is not null || result.Framework is not null || result.Overlays is not null))
        {
            throw new ArgumentException("coverage, framework and overlays are only supported by dotnet-options validation.");
        }
        return result;
    }

    // The generated record equality would compare the overlay array by reference.
    public bool Equals(ValidationConfiguration? other)
        => other is not null &&
            Type == other.Type &&
            Coverage == other.Coverage &&
            Framework == other.Framework &&
            (Overlays is null
                ? other.Overlays is null
                : other.Overlays is not null && Overlays.SequenceEqual(other.Overlays));

    public override int GetHashCode()
        => HashCode.Combine(Type, Coverage, Framework, Overlays?.Length ?? 0);
}

