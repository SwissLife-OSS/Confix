using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;

namespace Confix.Tool.Schema;

/// <summary>
/// Provides access to the keywords of a <see cref="JsonSchema"/>.
/// Since JsonSchema.Net 9 the schema no longer exposes typed keywords, therefore the values are
/// read from the underlying JSON.
/// </summary>
public static class JsonSchemaKeywordExtensions
{
    private static readonly ConditionalWeakTable<JsonSchema, SchemaCache> _cache = new();

    public static JsonNode? AsNode(this JsonSchema schema)
        => _cache.GetValue(schema, static s => new SchemaCache(s)).Node;

    public static JsonObject? AsObject(this JsonSchema schema)
        => schema.AsNode() as JsonObject;

    public static IReadOnlyDictionary<string, JsonSchema>? GetProperties(this JsonSchema schema)
        => schema.GetSchemaMap("properties");

    public static IReadOnlyDictionary<string, JsonSchema>? GetDefs(this JsonSchema schema)
        => schema.GetSchemaMap("$defs");

    public static IReadOnlyList<JsonSchema>? GetAnyOf(this JsonSchema schema)
        => schema.GetSchemaList("anyOf");

    public static IReadOnlyList<JsonSchema>? GetOneOf(this JsonSchema schema)
        => schema.GetSchemaList("oneOf");

    public static IReadOnlyList<JsonSchema>? GetAllOf(this JsonSchema schema)
        => schema.GetSchemaList("allOf");

    public static JsonSchema? GetItems(this JsonSchema schema)
        => schema.GetSubSchema("items");

    public static JsonSchema? GetAdditionalProperties(this JsonSchema schema)
        => schema.GetSubSchema("additionalProperties");

    public static JsonNode? GetDefault(this JsonSchema schema)
        => schema.GetKeywordValue("default");

    public static string? GetTitle(this JsonSchema schema)
        => schema.GetKeywordValue("title")?.GetValue<string>();

    public static string? GetDescription(this JsonSchema schema)
        => schema.GetKeywordValue("description")?.GetValue<string>();

    public static Uri? GetRef(this JsonSchema schema)
        => schema.GetKeywordValue("$ref")?.GetValue<string>() is { } reference
            ? new Uri(reference, UriKind.RelativeOrAbsolute)
            : null;

    public static IReadOnlyCollection<string>? GetRequired(this JsonSchema schema)
        => schema.GetKeywordValue("required") is JsonArray required
            ? required.OfType<JsonNode>().Select(x => x.GetValue<string>()).ToArray()
            : null;

    public static IReadOnlyList<JsonNode>? GetExamples(this JsonSchema schema)
        => schema.GetKeywordValue("examples") is JsonArray examples
            ? examples.OfType<JsonNode>().ToArray()
            : null;

    public static JsonArray? GetMetadata(this JsonSchema schema)
        => schema.GetKeywordValue(Confix.Entities.Schema.MetadataKeyword.Name) as JsonArray;

    public static SchemaValueType? GetJsonType(this JsonSchema schema)
    {
        var value = schema.GetKeywordValue("type");

        return value switch
        {
            JsonArray array => array
                .OfType<JsonNode>()
                .Select(x => ParseType(x.GetValue<string>()))
                .Aggregate(default(SchemaValueType), (current, next) => current | next),
            JsonValue => ParseType(value.GetValue<string>()),
            _ => null
        };
    }

    private static SchemaValueType ParseType(string value)
        => value switch
        {
            "array" => SchemaValueType.Array,
            "boolean" => SchemaValueType.Boolean,
            "integer" => SchemaValueType.Integer,
            "null" => SchemaValueType.Null,
            "number" => SchemaValueType.Number,
            "object" => SchemaValueType.Object,
            "string" => SchemaValueType.String,
            _ => throw new JsonSchemaException($"Unknown schema value type '{value}'.")
        };

    private static JsonNode? GetKeywordValue(this JsonSchema schema, string keyword)
        => schema.AsObject() is { } obj && obj.TryGetPropertyValue(keyword, out var value)
            ? value
            : null;

    private static JsonSchema? GetSubSchema(this JsonSchema schema, string keyword)
        => _cache.GetValue(schema, static s => new SchemaCache(s)).GetSubSchema(keyword);

    private static IReadOnlyDictionary<string, JsonSchema>? GetSchemaMap(
        this JsonSchema schema,
        string keyword)
        => _cache.GetValue(schema, static s => new SchemaCache(s)).GetSchemaMap(keyword);

    private static IReadOnlyList<JsonSchema>? GetSchemaList(this JsonSchema schema, string keyword)
        => _cache.GetValue(schema, static s => new SchemaCache(s)).GetSchemaList(keyword);

    /// <summary>
    /// Caches the JSON representation of a schema as well as the sub schemas that were created
    /// from it, so that the same schema instances are returned on every access.
    /// </summary>
    private sealed class SchemaCache
    {
        private readonly Dictionary<string, object?> _keywords = new();

        public SchemaCache(JsonSchema schema)
        {
            Node = JsonSerializer.SerializeToNode(schema);
        }

        public JsonNode? Node { get; }

        public JsonSchema? GetSubSchema(string keyword)
            => (JsonSchema?)GetOrCreate(keyword, static node => ToSchema(node));

        public IReadOnlyDictionary<string, JsonSchema>? GetSchemaMap(string keyword)
            => (IReadOnlyDictionary<string, JsonSchema>?)GetOrCreate(keyword,
                static node => node is JsonObject obj
                    ? obj
                        .Where(x => x.Value is not null)
                        .ToDictionary(x => x.Key, x => ToSchema(x.Value)!)
                    : null);

        public IReadOnlyList<JsonSchema>? GetSchemaList(string keyword)
            => (IReadOnlyList<JsonSchema>?)GetOrCreate(keyword,
                static node => node is JsonArray array
                    ? array.Select(x => ToSchema(x)!).OfType<JsonSchema>().ToArray()
                    : null);

        private object? GetOrCreate(string keyword, Func<JsonNode?, object?> factory)
        {
            if (_keywords.TryGetValue(keyword, out var cached))
            {
                return cached;
            }

            JsonNode? value = null;
            if (Node is JsonObject obj)
            {
                obj.TryGetPropertyValue(keyword, out value);
            }

            var created = value is null ? null : factory(value);
            _keywords[keyword] = created;

            return created;
        }

        private static JsonSchema? ToSchema(JsonNode? node)
            => node is null ? null : ConfixJsonSchema.FromText(node.ToJsonString());
    }
}
