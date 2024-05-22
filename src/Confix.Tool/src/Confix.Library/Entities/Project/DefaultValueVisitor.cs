using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Text.Json.Nodes;
using Confix.Tool.Schema;
using Json.More;
using Json.Schema;

namespace Confix.Tool.Entities.Components.DotNet;

public sealed class DefaultValueVisitor : JsonDocumentRewriter<DefaultValueVisitorContext>
{
    protected override JsonNode Rewrite(JsonObject obj, DefaultValueVisitorContext context)
    {
        obj = obj.Copy()!.AsObject();
        var schema = context.Schemas.Peek();
        var possibleSchemas = schema.GetPossibleSchemas(context.ReferenceResolver).ToArray();

        // initialize missing required fields
        var properties = possibleSchemas
            .Select(x => x.GetProperties())
            .OfType<IReadOnlyDictionary<string, JsonSchema>>()
            .SingleOrNone();

        if (properties is null)
        {
            // we don't know what to do with this object
            return obj;
        }

        var requiredProperties = possibleSchemas
            .Select(x => x.GetRequired())
            .OfType<IReadOnlyCollection<string>>()
            .Where(x => x.Count > 0)
            .SingleOrNone();

        // we are only interested in the properties that are present on the json schema
        foreach (var (field, propertySchema) in properties)
        {
            // cyclic dependency protection
            if (context.Schemas.Contains(propertySchema))
            {
                continue;
            }

            var possiblePropertySchemas = propertySchema
                .GetPossibleSchemas(context.ReferenceResolver)
                .ToArray();

            context.Schemas.Push(propertySchema);

            // when the field is null and we have a default value, set it
            if (!obj.ContainsKey(field))
            {
                if (possiblePropertySchemas.GetDefaultValue() is { } defaultValue)
                {
                    obj[field] = defaultValue;
                }
                else if (requiredProperties != null && requiredProperties.Contains(field))
                {
                    obj[field] = propertySchema.IsArray()
                        ? new JsonArray()
                        : possiblePropertySchemas
                            .Where(x => x.GetProperties() is { Count: > 0 })
                            .SingleOrNone() is not null
                            ? new JsonObject()
                            : null;
                }
            }

            // when the field is null and we have a required field, initialize it
            if (obj.ContainsKey(field) &&
                obj[field] is null &&
                requiredProperties?.Contains(field) is true)
            {
                obj[field] = propertySchema.IsArray()
                    ? new JsonArray()
                    : possiblePropertySchemas
                        .Where(x => x.GetRequired() is { Count: > 0 })
                        .SingleOrNone() is not null
                        ? new JsonObject()
                        : null;
            }

            if (obj[field] is { } elm)
            {
                obj[field] = Rewrite(elm, context);
            }

            context.Schemas.Pop();
        }

        return obj;
    }

    /// <inheritdoc />
    protected override JsonNode Rewrite(JsonArray array, DefaultValueVisitorContext context)
    {
        var newArray = array.Copy()!.AsArray();
        var schema = context.Schemas.Peek();
        var possibleSchemas = schema.GetPossibleSchemas(context.ReferenceResolver).ToArray();

        var itemSchema = possibleSchemas.Select(x => x.GetItems()).SingleOrNone();

        if (itemSchema is null)
        {
            // we don't know the type of the array
            return newArray;
        }

        context.Schemas.Push(itemSchema);

        for (var index = 0; index < array.Count; index++)
        {
            if (array[index] is { } e)
            {
                newArray[index] = Rewrite(e, context);
            }
            else
            {
                newArray[index] = null;
            }
        }

        context.Schemas.Pop();

        return newArray;
    }

    public static DefaultValueVisitor Instance { get; } = new();

    public static JsonNode ApplyDefaults(JsonSchema schema, JsonNode node)
    {
        var context = DefaultValueVisitorContext.From(schema);
        return Instance.Rewrite(node, context);
    }
}

file static class Extensions
{
    public static IEnumerable<JsonSchema> GetPossibleSchemas(
        this JsonSchema schema,
        ISchemaReferenceResolver resolver)
    {
        var todo = new Queue<JsonSchema>();
        todo.Enqueue(schema);

        while (todo.TryDequeue(out var currentSchema))
        {
            // we are not interested in variables
            if (currentSchema.IsConfixVariableReference())
            {
                continue;
            }

            // resolve the reference
            if (currentSchema.GetRef() is { } @ref && resolver.Resolve(@ref) is { } resolveRef)
            {
                todo.Enqueue(resolveRef);
            }

            todo.EnqueueMany(currentSchema.GetAnyOf());
            todo.EnqueueMany(currentSchema.GetOneOf());
            todo.EnqueueMany(currentSchema.GetAllOf());

            yield return currentSchema;
        }
    }

    public static JsonNode? GetDefaultValue(this IEnumerable<JsonSchema> schemas)
        => schemas
            .Select(x => x.GetDefault())
            .OfType<JsonNode>()
            .SingleOrNone();

    public static JsonObject AddRequiredFields(this JsonObject value, JsonSchema schema)
    {
        var properties = schema.GetProperties() ?? ImmutableDictionary<string, JsonSchema>.Empty;
        var requiredProperties = schema.GetRequired() ?? Array.Empty<string>();

        foreach (var required in requiredProperties.Distinct())
        {
            if (value.ContainsKey(required))
            {
                continue;
            }

            if (properties.TryGetValue(required, out var propertySchema))
            {
                value[required] = propertySchema.IsArray() ? new JsonArray() : null;
            }
        }

        return value;
    }

    public static bool IsConfixVariableReference(this JsonSchema schema)
        => schema.GetRef() is
        {
            OriginalString: ProjectComposer.References.Urls.ConfixVariables
        };

    public static void EnqueueMany<T>(
        this Queue<T> current,
        IEnumerable<T>? other)
    {
        foreach (var item in other ?? Array.Empty<T>())
        {
            current.Enqueue(item);
        }
    }
}
