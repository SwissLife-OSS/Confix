using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Configuration;

namespace Confix.Runner;

internal static class SchemaExport
{
    private const string SchemaDialect = "https://json-schema.org/draft/2020-12/schema";

    /// <summary>Matches a Confix variable expression such as <c>$secret:name</c>.</summary>
    private const string VariablePattern = "^\\$[^:]+:.+";

    public static JsonObject Export(IEnumerable<IConfixContract> contracts, bool strict)
    {
        var options = new JsonSerializerOptions { TypeInfoResolver = CreateResolver() };
        var root = CreateObject(strict);
        root["$schema"] = SchemaDialect;

        foreach (var contract in contracts)
        {
            var schema = options.GetJsonSchemaAsNode(contract.OptionsType, CreateExporterOptions());

            if (contract.Section.Length == 0)
            {
                var exported = schema.AsObject();
                exported["$schema"] = SchemaDialect;
                RewriteReferences(exported, "#");

                return exported;
            }

            Mount(root, schema, contract, strict);
        }

        return root;
    }

    private static DefaultJsonTypeInfoResolver CreateResolver()
    {
        var resolver = new DefaultJsonTypeInfoResolver();

        resolver.Modifiers.Add(static info =>
        {
            if (info.Kind != JsonTypeInfoKind.Object)
            {
                return;
            }

            // Configuration binding does not follow JsonIgnore/JsonPropertyName attributes.
            info.Properties.Clear();

            var members = info.Type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetIndexParameters().Length == 0 && p.GetMethod?.IsPublic == true);

            foreach (var member in members)
            {
                var name = member.GetCustomAttribute<ConfigurationKeyNameAttribute>()?.Name
                    ?? member.Name;
                var property = info.CreateJsonPropertyInfo(member.PropertyType, name);

                property.AttributeProvider = member;
                property.Get = member.GetValue;
                property.IsRequired = member.IsDefined(typeof(ConfixRequiredKeyAttribute));

                if (member.SetMethod?.IsPublic == true)
                {
                    property.Set = member.SetValue;
                }

                info.Properties.Add(property);
            }
        });

        return resolver;
    }

    private static JsonSchemaExporterOptions CreateExporterOptions()
    {
        return new JsonSchemaExporterOptions
        {
            TreatNullObliviousAsNonNullable = false,
            TransformSchemaNode = static (context, node) =>
            {
                if (node is not JsonObject obj)
                {
                    return node;
                }

                if (context.TypeInfo.Kind == JsonTypeInfoKind.Object)
                {
                    obj["additionalProperties"] = false;
                }

                if (context.PropertyInfo?.AttributeProvider is not PropertyInfo member)
                {
                    return obj;
                }

                ApplyAnnotations(obj, member);

                // Any value may instead be a variable expression that Confix resolves on build.
                return new JsonObject
                {
                    ["anyOf"] = new JsonArray(
                        obj,
                        new JsonObject { ["type"] = "string", ["pattern"] = VariablePattern })
                };
            }
        };
    }

    private static void ApplyAnnotations(JsonObject schema, PropertyInfo member)
    {
        if (member.GetCustomAttribute<DescriptionAttribute>() is { } description)
        {
            schema["description"] = description.Description;
        }

        if (member.GetCustomAttribute<RangeAttribute>() is { Minimum: int min, Maximum: int max })
        {
            schema["minimum"] = min;
            schema["maximum"] = max;
        }

        var isString = member.PropertyType == typeof(string);

        if (member.GetCustomAttribute<MinLengthAttribute>() is { } minimum)
        {
            schema[isString ? "minLength" : "minItems"] = minimum.Length;
        }

        if (member.GetCustomAttribute<MaxLengthAttribute>() is { } maximum)
        {
            schema[isString ? "maxLength" : "maxItems"] = maximum.Length;
        }

        if (member.GetCustomAttribute<StringLengthAttribute>() is { } length)
        {
            schema["minLength"] = length.MinimumLength;
            schema["maxLength"] = length.MaximumLength;
        }

        if (isString && member.GetCustomAttribute<RequiredAttribute>() is { AllowEmptyStrings: false })
        {
            schema["type"] = "string";
            schema["minLength"] = 1;
            schema["pattern"] = "\\S";
        }
    }

    private static void Mount(
        JsonObject root,
        JsonNode schema,
        IConfixContract contract,
        bool strict)
    {
        var segments = contract.Section.Split(':');

        // Keep exporter-local references valid after mounting the contract beneath the root.
        var prefix = "#/" + string.Join('/', segments.Select(s => "properties/" + Escape(s)));
        RewriteReferences(schema, prefix);

        var current = root;

        for (var i = 0; i < segments.Length; i++)
        {
            var properties = current["properties"]!.AsObject();

            if (contract.Required)
            {
                var required = current["required"]!.AsArray();

                if (!required.Any(n => n?.GetValue<string>() == segments[i]))
                {
                    required.Add(segments[i]);
                }
            }

            if (i == segments.Length - 1)
            {
                properties[segments[i]] = schema;
            }
            else
            {
                current = (properties[segments[i]] ??= CreateObject(strict)).AsObject();
            }
        }
    }

    private static JsonObject CreateObject(bool strict)
    {
        return new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject(),
            ["required"] = new JsonArray(),
            ["additionalProperties"] = !strict
        };
    }

    private static string Escape(string value)
    {
        return value.Replace("~", "~0").Replace("/", "~1");
    }

    /// <summary>
    /// Wrapping each property in an anyOf adds a level the exporter's own pointers do not know
    /// about, so every traversal through a property has to step into the first alternative.
    /// </summary>
    private static string AdaptPointer(string pointer)
    {
        var segments = pointer.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var adapted = new StringBuilder();

        for (var i = 0; i < segments.Length; i++)
        {
            adapted.Append('/').Append(segments[i]);

            if (segments[i] != "properties" || i + 1 >= segments.Length)
            {
                continue;
            }

            adapted.Append('/').Append(segments[++i]);

            if (i + 1 < segments.Length)
            {
                adapted.Append("/anyOf/0");
            }
        }

        return adapted.ToString();
    }

    private static void RewriteReferences(JsonNode? node, string prefix)
    {
        switch (node)
        {
            case JsonObject obj:
                if (obj["$ref"] is JsonValue reference &&
                    reference.TryGetValue<string>(out var value) &&
                    value.StartsWith('#'))
                {
                    obj["$ref"] = prefix + AdaptPointer(value[1..]);
                }

                foreach (var property in obj.ToArray())
                {
                    RewriteReferences(property.Value, prefix);
                }

                break;

            case JsonArray array:
                foreach (var item in array)
                {
                    RewriteReferences(item, prefix);
                }

                break;
        }
    }
}
