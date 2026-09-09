using Confix.Entities.Schema;
using Json.Schema;

namespace Confix.Tool.Schema;

/// <summary>
/// Loads and builds JSON schemas in isolation from each other.
/// </summary>
/// <remarks>
/// JsonSchema.Net registers every built schema in a registry keyed by its URI and refuses to
/// overwrite an existing entry. Confix loads the same schema files repeatedly (once per pipeline
/// run), so each load gets its own registry and a unique base URI.
/// </remarks>
public static class ConfixJsonSchema
{
    private static readonly Dialect _dialect = Dialect.Default.With(
        [MetadataKeyword.Instance],
        allowUnknownKeywords: true);

    /// <summary>
    /// Creates build options backed by a private schema registry.
    /// </summary>
    public static BuildOptions CreateBuildOptions()
        => new() { Dialect = _dialect };

    /// <summary>
    /// Creates a base URI that is unique to a single build.
    /// </summary>
    public static Uri CreateBaseUri()
        => new($"https://schemas.confix.dev/{Guid.NewGuid():N}");

    /// <summary>
    /// Loads a schema from a file without registering it globally.
    /// </summary>
    public static JsonSchema FromFile(string fileName)
        => JsonSchema.FromFile(fileName, CreateBuildOptions(), CreateBaseUri());

    /// <summary>
    /// Builds a schema from text without registering it globally.
    /// </summary>
    public static JsonSchema FromText(string jsonText)
        => JsonSchema.FromText(jsonText, CreateBuildOptions(), CreateBaseUri());

    /// <summary>
    /// Builds the schema without registering it globally.
    /// </summary>
    public static JsonSchema BuildIsolated(this JsonSchemaBuilder builder)
        => builder.Build(CreateBuildOptions(), CreateBaseUri());
}
