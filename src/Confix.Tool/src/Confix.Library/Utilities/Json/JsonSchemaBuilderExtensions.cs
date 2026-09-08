using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Entities.Schema;
using Confix.Utilities.Json;
using Json.Schema;

namespace Confix.Tool.Schema;

public static class JsonSchemaBuilderExtensions
{
    public static readonly JsonSchema Null = new JsonSchemaBuilder().Type(SchemaValueType.Null);

    public static JsonSchemaBuilder Nullable(this JsonSchemaBuilder builder, bool isNull = true)
        => isNull
            ? new JsonSchemaBuilder().AnyOf(builder.BuildIsolated(), Null)
            : builder;

    public static JsonSchemaBuilder HasVariables(
        this JsonSchemaBuilder builder,
        bool hasVariables = true)
        => hasVariables
            ? builder.Unrecognized(JsonSchemaProperties.HasVariable, true)
            : builder;

    public static JsonSchemaBuilder WithDescription(
        this JsonSchemaBuilder builder,
        string? description)
    {
        if (description is not null)
        {
            builder.Description(description);
        }

        return builder;
    }

    public static JsonSchemaBuilder WithDefault(
        this JsonSchemaBuilder builder,
        JsonNode? defaultValue)
    {
        if (defaultValue is not null)
        {
            builder.Default(defaultValue);
        }

        return builder;
    }

    public static JsonSchemaBuilder WithMetadata(
        this JsonSchemaBuilder builder,
        JsonArray? metadata)
    {
        if (metadata is not null)
        {
            var value = new JsonArray();
            foreach (var item in metadata)
            {
                if (item is not null)
                {
                    value.Add(item.Copy());
                }
            }

            builder.Add(MetadataKeyword.Name, value);
        }

        return builder;
    }

    public static JsonSchemaBuilder AddComponentName(
        this JsonSchemaBuilder builder,
        string componentName)
    {
        return builder.Unrecognized(JsonSchemaProperties.ComponentName, componentName);
    }

    public static JsonSchemaBuilder Properties(
        this JsonSchemaBuilder builder,
        IReadOnlyDictionary<string, JsonSchema> properties)
        => builder.Set("properties", ToObject(properties));

    public static JsonSchemaBuilder Properties(
        this JsonSchemaBuilder builder,
        params (string Name, JsonSchema Schema)[] properties)
        => builder.Properties(properties.ToDictionary(x => x.Name, x => x.Schema));

    public static JsonSchemaBuilder Defs(
        this JsonSchemaBuilder builder,
        IReadOnlyDictionary<string, JsonSchema> defs)
        => builder.Set("$defs", ToObject(defs));

    public static JsonSchemaBuilder AnyOf(
        this JsonSchemaBuilder builder,
        IEnumerable<JsonSchema> schemas)
        => builder.Set("anyOf", ToArray(schemas));

    public static JsonSchemaBuilder AnyOf(
        this JsonSchemaBuilder builder,
        params JsonSchema[] schemas)
        => builder.AnyOf((IEnumerable<JsonSchema>) schemas);

    public static JsonSchemaBuilder AdditionalProperties(
        this JsonSchemaBuilder builder,
        JsonSchema schema)
        => builder.Set("additionalProperties", ToNode(schema));

    public static JsonSchemaBuilder Items(
        this JsonSchemaBuilder builder,
        JsonSchema schema)
        => builder.Set("items", ToNode(schema));

    private static JsonSchemaBuilder Set(
        this JsonSchemaBuilder builder,
        string keyword,
        JsonNode value)
    {
        builder.Add(keyword, value);

        return builder;
    }

    private static JsonNode ToNode(JsonSchema schema)
        => JsonSerializer.SerializeToNode(schema)!;

    private static JsonObject ToObject(IEnumerable<KeyValuePair<string, JsonSchema>> schemas)
    {
        var result = new JsonObject();
        foreach (var (name, schema) in schemas)
        {
            result[name] = ToNode(schema);
        }

        return result;
    }

    private static JsonArray ToArray(IEnumerable<JsonSchema> schemas)
        => new(schemas.Select(ToNode).ToArray());
}
