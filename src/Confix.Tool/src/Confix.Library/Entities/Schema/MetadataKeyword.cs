using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using Json.Schema;

namespace Confix.Entities.Schema;

/// <summary>
/// Handles `default`.
/// </summary>
[SchemaKeyword(Name)]
[JsonConverter(typeof(MetadataKeywordJsonConverter))]
public class MetadataKeyword : IJsonSchemaKeyword, IEquatable<MetadataKeyword>
{
    /// <summary>
    /// The JSON name of the keyword.
    /// </summary>
    public const string Name = "metadata";

    /// <summary>
    /// The value to use as the default.
    /// </summary>
    public JsonArray Value { get; }

    /// <summary>
    /// Creates a new <see cref="MetadataKeyword"/>.
    /// </summary>
    /// <param name="value">The value to use as the default.</param>
    public MetadataKeyword(JsonArray value)
    {
        Value = value;
    }

    /// <summary>
    /// Performs evaluation for the keyword.
    /// </summary>
    /// <param name="context">Contextual details for the evaluation process.</param>
    public void Evaluate(EvaluationContext context)
    {
        context.EnterKeyword(Name);
        context.LocalResult.SetAnnotation(Name, Value);
        context.ExitKeyword(Name, true);
    }

    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
    public bool Equals(MetadataKeyword? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value.IsEquivalentTo(other.Value);
    }

    /// <summary>Determines whether the specified object is equal to the current object.</summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as MetadataKeyword);
    }

    /// <summary>Serves as the default hash function.</summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return Value?.GetEquivalenceHashCode() ?? 0;
    }
}

internal class MetadataKeywordJsonConverter : JsonConverter<MetadataKeyword>
{
    public override MetadataKeyword Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var node = JsonSerializer.Deserialize<JsonArray>(ref reader, options);

        return new MetadataKeyword(node);
    }

    public override void Write(
        Utf8JsonWriter writer,
        MetadataKeyword value,
        JsonSerializerOptions options)
    {
        writer.WritePropertyName(MetadataKeyword.Name);
        JsonSerializer.Serialize(writer, value.Value, options);
    }
}
