using System.Text.Json.Nodes;
using Confix.Entities.Schema;
using HotChocolate;
using HotChocolate.Types;
using Json.More;
using Json.Schema;
using static Json.Schema.SchemaValueType;

namespace Confix.Tool.Schema;

public static class SchemaExtensions
{
    public static JsonSchemaBuilder ToJsonSchema(this ISchema schema)
        => schema.QueryType
            .ToTypeDefinition()
            .Schema(MetaSchemas.Draft202012Id)
            .Type(SchemaValueType.Object)
            .Defs(schema.Types
                .Where(x => !x.IsIntrospectionType())
                .ToDictionary(t => t.Name, t => t.ToTypeDefinition(schema).Build()));

    private static JsonSchemaBuilder ToTypeDefinition(this INamedType type, ISchema schema)
        => type switch
        {
            ObjectType t => t.ToTypeDefinition(),
            InputObjectType t => throw t.ToException(),
            UnionType t => t.ToTypeDefinition(schema),
            InterfaceType t => t.ToTypeDefinition(schema),
            ScalarType t => t.ToTypeDefinition(),
            EnumType t => t.ToTypeDefinition(),
            _ => throw new NotImplementedException()
        };

    private static bool IsIntrospectionType(this INamedType type)
        => type.Name.StartsWith("__");

    private static JsonSchemaBuilder ToTypeDefinition(this UnionType type, ISchema schema)
        => type.ToAbstractTypeDefinition(schema);

    private static JsonSchemaBuilder ToTypeDefinition(this InterfaceType type, ISchema schema)
        => type.ToAbstractTypeDefinition(schema);

    private static JsonSchemaBuilder ToTypeDefinition(this EnumType type)
        => new JsonSchemaBuilder()
            .Type(SchemaValueType.String)
            .Enum(type.Values.Select(v => v.Name))
            .WithDescription(type.Description);

    private static JsonSchemaBuilder ToTypeDefinition(this ObjectType type)
        => new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Properties(type.Fields
                .Where(x => !x.IsIntrospectionField)
                .ToDictionary(x => x.Name, x => x.ToTypeReference().Build()))
            .Required(type.Fields
                .Where(x => !x.IsIntrospectionField && x.Type.IsNonNullType())
                .Select(x => x.Name))
            .AdditionalProperties(false)
            .WithDescription(type.Description);

    private static JsonSchemaBuilder ToTypeReference(this IOutputField field)
        => field.Type
            .ToTypeReferenceBuilder()
            .Deprecated(field.IsDeprecated)
            .WithDefault(field.DefaultValueNode())
            .WithMetadata(field.GetMetadata())
            .WithDescription(field.Description);

    private static JsonNode? DefaultValueNode(this IOutputField field)
    {
        var defaultValue = field.Directives.Where(x => x.Type.Name == DefaultValueDirective.Name)
            .Select(x => x.AsValue<DefaultValueDirective>())
            .FirstOrDefault();

        return defaultValue?.Value.AsNode();
    }

    private static JsonArray? GetMetadata(this IOutputField field)
    {
        var metadata = field.Directives
            .Where(x => x.Type.Name == MetadataDirective.Name)
            .Select(x => x.AsValue<MetadataDirective>())
            .Select(x => x.Value.AsNode())
            .OfType<JsonNode>();

        var dependencies = field.Directives
            .Where(x => x.Type.Name == DependencyDirective.Name)
            .Select(x => x.AsValue<DependencyDirective>())
            .Select(x => new JsonObject { ["type"] = "dependency", ["kind"] = x.Kind })
            .OfType<JsonNode>();

        var result = metadata.Concat(dependencies).ToArray();
        if (result.Length == 0)
        {
            return null;
        }

        return new JsonArray(result);
    }

    private static JsonSchemaBuilder ToTypeReference(this IType type, bool required = false)
        => type.ToTypeReferenceBuilder(required);

    private static JsonSchemaBuilder ToTypeReferenceBuilder(this IType type, bool required = false)
        => type switch
        {
            NonNullType t => t.Type.ToTypeReferenceBuilder(true),

            ListType t => new JsonSchemaBuilder()
                .Type(SchemaValueType.Array)
                .Items(t.ElementType.ToTypeReference())
                .Nullable(!required),

            INamedType t => new JsonSchemaBuilder()
                .Ref($"#/$defs/{t.Name}")
                .Nullable(!required),

            _ => throw new NotImplementedException()
        };

    private static JsonSchemaBuilder ToTypeDefinition(this ScalarType type) => (type.Name switch
        {
            "Int" or "Long" => new JsonSchemaBuilder().Type(Integer),
            "Float" or "Double" => new JsonSchemaBuilder().Type(Number),
            "Boolean" => new JsonSchemaBuilder().Type(SchemaValueType.Boolean),
            "String" => new JsonSchemaBuilder().Type(SchemaValueType.String),
            "Uuid" or "UUID" => new JsonSchemaBuilder().Type(SchemaValueType.String).Format("uuid"),
            "Guid" or "GUID" => new JsonSchemaBuilder().Type(SchemaValueType.String).Format("uuid"),

            "Date" => new JsonSchemaBuilder().Type(SchemaValueType.String).Format("date"),
            "DateTime" => new JsonSchemaBuilder().Type(SchemaValueType.String).Format("date-time"),
            "TimeSpan" => new JsonSchemaBuilder().Type(SchemaValueType.String).Format("time-span"),
            "Duration" => new JsonSchemaBuilder().Type(SchemaValueType.String).Format("duration"),

            "EmailAddress" => new JsonSchemaBuilder().Type(SchemaValueType.String).Format("email"),
            "IdnEmailAddress" => new JsonSchemaBuilder()
                .Type(SchemaValueType.String)
                .Format("idn-email"),

            "HostName" => new JsonSchemaBuilder().Type(SchemaValueType.String).Format("hostname"),
            "IdnHostName" => new JsonSchemaBuilder()
                .Type(SchemaValueType.String)
                .Format("idn-hostname"),

            "IpAddress" or "Ip" or "Ipv4" => new JsonSchemaBuilder()
                .Type(SchemaValueType.String)
                .Format("ipv4"),

            "Ipv6" => new JsonSchemaBuilder()
                .Type(SchemaValueType.String)
                .Format("ipv6"),

            "Uri" => new JsonSchemaBuilder().Type(SchemaValueType.String).Format("uri"),
            "Url" => new JsonSchemaBuilder().Type(SchemaValueType.String).Format("uri"),

            "JsonPointer" => new JsonSchemaBuilder()
                .Type(SchemaValueType.String)
                .Format("json-pointer"),

            "RegEx" or "Regex" => new JsonSchemaBuilder()
                .Type(SchemaValueType.String)
                .Format("regex"),

            "Json" or "Any" => new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .AdditionalProperties(true)
                .Format("json"),

            // default to string
            _ => new JsonSchemaBuilder().Type(SchemaValueType.String),
        })
        .HasVariables()
        .WithDescription(type.Description);

    private static JsonSchemaBuilder ToAbstractTypeDefinition(this INamedType type, ISchema schema)
        => new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .AnyOf(schema.GetPossibleTypes(type).Select(t => t.ToTypeReference().Build()))
            .WithDescription(type.Description);

    private static Exception ToException(this InputObjectType t) =>
        throw new ExitException("Input object types are not supported.")
        {
            Help = $"""
                You probably want to use an object type instead.
                Check the type {t.Name} on ({t.SyntaxNode?.Location?.Line},{t.SyntaxNode?.Location?.Column})
                """
        };
}
