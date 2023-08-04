using System.Text.Json.Nodes;
using Json.Schema;

namespace Confix.Tool.Schema;

public static class JsonSchemaBuilderExtensions
{
    public static readonly JsonSchema Null = new JsonSchemaBuilder().Type(SchemaValueType.Null);

    public static JsonSchemaBuilder Nullable(this JsonSchemaBuilder builder, bool isNull = true)
        => isNull
            ? new JsonSchemaBuilder().AnyOf(builder, Null)
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

    public static JsonSchemaBuilder AddComponentName(
        this JsonSchemaBuilder builder,
        string componentName)
    {
        return builder.Unrecognized(JsonSchemaProperties.ComponentName, componentName);
    }
}
