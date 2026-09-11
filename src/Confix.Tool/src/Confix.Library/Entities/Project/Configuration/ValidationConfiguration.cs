using System.Text.Json.Nodes;

namespace Confix.Tool.Abstractions;

/// <summary>Selects configuration validation independently of component input formats.</summary>
public sealed record ValidationConfiguration(string? Type = null, string? Coverage = null,
    string? Framework = null, string[]? Overlays = null)
{
    [System.Text.Json.Serialization.JsonIgnore]
    public string EffectiveType => Type ?? "json-schema";

    public bool ShouldSerializeEffectiveType() => false;

    public ValidationConfiguration Merge(ValidationConfiguration? other)
    {
        if (other is null) return this;
        // Provider-specific settings must not leak when switching providers.
        if (other.Type is not null && Type is not null && other.Type != Type) return other;
        return new(other.Type ?? Type, other.Coverage ?? Coverage,
            other.Framework ?? Framework, other.Overlays ?? Overlays);
    }

    public static ValidationConfiguration? Parse(JsonNode? node)
    {
        if (node is null) return null;
        if (node is not JsonObject obj) throw new ArgumentException("project.validation must be an object.");
        if (obj.Any(p => p.Key is not ("type" or "coverage" or "framework" or "overlays")))
            throw new ArgumentException("Unknown project.validation setting.");
        var result = new ValidationConfiguration(obj["type"]?.GetValue<string>(), obj["coverage"]?.GetValue<string>(),
            obj["framework"]?.GetValue<string>(), obj["overlays"]?.AsArray().Select(n => n!.GetValue<string>()).ToArray());
        if (result.Type is not (null or "json-schema" or "dotnet-options"))
            throw new ArgumentException("project.validation.type must be json-schema or dotnet-options.");
        if (result.Coverage is not (null or "strict" or "registeredSections"))
            throw new ArgumentException("project.validation.coverage must be strict or registeredSections.");
        if (result.Type == "json-schema" && (result.Coverage is not null || result.Framework is not null || result.Overlays is not null))
            throw new ArgumentException("coverage, framework and overlays are only supported by dotnet-options validation.");
        return result;
    }
}
