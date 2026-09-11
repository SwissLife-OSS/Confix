using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using Confix;
using Microsoft.Extensions.Configuration;

internal static class SchemaExport
{
    public static JsonObject Export(IEnumerable<IConfixContract> contracts, bool strict)
    {
        var resolver = new DefaultJsonTypeInfoResolver();
        resolver.Modifiers.Add(info =>
        {
            if (info.Kind != JsonTypeInfoKind.Object) return;
            info.Properties.Clear();
            foreach (var member in info.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetIndexParameters().Length == 0 && p.GetMethod?.IsPublic == true))
            {
                // Configuration binding does not follow JsonIgnore/JsonPropertyName attributes.
                var property = info.CreateJsonPropertyInfo(member.PropertyType,
                    member.GetCustomAttribute<ConfigurationKeyNameAttribute>()?.Name ?? member.Name);
                property.AttributeProvider = member;
                property.Get = member.GetValue;
                if (member.SetMethod?.IsPublic == true) property.Set = member.SetValue;
                property.IsRequired = member.IsDefined(typeof(ConfixRequiredKeyAttribute));
                info.Properties.Add(property);
            }
        });
        var options = new JsonSerializerOptions { TypeInfoResolver = resolver };
        var root = Object(strict);
        root["$schema"] = "https://json-schema.org/draft/2020-12/schema";
        foreach (var contract in contracts)
        {
            var schema = options.GetJsonSchemaAsNode(contract.OptionsType, new JsonSchemaExporterOptions
            {
                TreatNullObliviousAsNonNullable = false,
                TransformSchemaNode = (context, node) =>
                {
                    if (node is not JsonObject obj) return node;
                    if (context.TypeInfo.Kind == JsonTypeInfoKind.Object) obj["additionalProperties"] = false;
                    if (context.PropertyInfo?.AttributeProvider is PropertyInfo member)
                    {
                        if (member.GetCustomAttribute<DescriptionAttribute>() is { } description) obj["description"] = description.Description;
                        if (member.GetCustomAttribute<RangeAttribute>() is { } range && range.Minimum is int min && range.Maximum is int max)
                        { obj["minimum"] = min; obj["maximum"] = max; }
                        if (member.GetCustomAttribute<MinLengthAttribute>() is { } minimum)
                            obj[member.PropertyType == typeof(string) ? "minLength" : "minItems"] = minimum.Length;
                        if (member.GetCustomAttribute<MaxLengthAttribute>() is { } maximum)
                            obj[member.PropertyType == typeof(string) ? "maxLength" : "maxItems"] = maximum.Length;
                        if (member.GetCustomAttribute<StringLengthAttribute>() is { } length)
                        { obj["minLength"] = length.MinimumLength; obj["maxLength"] = length.MaximumLength; }
                        if (member.GetCustomAttribute<RequiredAttribute>() is { AllowEmptyStrings: false } && member.PropertyType == typeof(string))
                        { obj["type"] = "string"; obj["minLength"] = 1; obj["pattern"] = "\\S"; }
                        obj = new JsonObject { ["anyOf"] = new JsonArray(obj, new JsonObject { ["type"] = "string", ["pattern"] = "^\\$[^:]+:.+" }) };
                    }
                    return obj;
                }
            });
            if (contract.Section.Length == 0)
            {
                var exported = schema.AsObject();
                exported["$schema"] = "https://json-schema.org/draft/2020-12/schema";
                return exported;
            }
            // Keep exporter-local references valid after mounting the contract beneath the root.
            RewriteReferences(schema, "#/" + string.Join('/', contract.Section.Split(':').Select(s => "properties/" + Escape(s))));
            var current = root;
            var segments = contract.Section.Split(':');
            for (var i = 0; i < segments.Length; i++)
            {
                var properties = current["properties"]!.AsObject();
                if (contract.Required)
                {
                    var required = current["required"]!.AsArray();
                    if (!required.Any(n => n?.GetValue<string>() == segments[i])) required.Add(segments[i]);
                }
                if (i == segments.Length - 1) properties[segments[i]] = schema;
                else current = (properties[segments[i]] ??= Object(strict)).AsObject();
            }
        }
        return root;
    }
    private static JsonObject Object(bool strict) => new() { ["type"] = "object", ["properties"] = new JsonObject(), ["required"] = new JsonArray(), ["additionalProperties"] = !strict };
    private static string Escape(string value) => value.Replace("~", "~0").Replace("/", "~1");
    private static void RewriteReferences(JsonNode? node, string prefix)
    {
        if (node is JsonObject obj)
        {
            if (obj["$ref"] is JsonValue reference && reference.TryGetValue<string>(out var value) && value.StartsWith('#')) obj["$ref"] = prefix + value[1..];
            foreach (var property in obj.ToArray()) RewriteReferences(property.Value, prefix);
        }
        else if (node is JsonArray array) foreach (var item in array) RewriteReferences(item, prefix);
    }
}
