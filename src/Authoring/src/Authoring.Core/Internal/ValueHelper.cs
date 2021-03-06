using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using HotChocolate;
using HotChocolate.Types;

namespace Confix.Authoring.Internal
{
    public static class ValueHelper
    {
        public static Dictionary<string, object?> CreateDefaultObjectValue(IType type)
        {
            var obj = new Dictionary<string, object?>();
            var objectType = (ObjectType)type.NamedType();

            foreach (var field in objectType.Fields)
            {
                if (field.IsIntrospectionField)
                {
                    continue;
                }

                obj[field.Name] = CreateDefaultValue(field.Type);
            }

            return obj;
        }

        private static object? CreateDefaultValue(IType type)
        {
            if (type.IsNonNullType())
            {
                if (type.IsListType())
                {
                    return CreateDefaultListValue(type);
                }

                if (type.IsObjectType())
                {
                    return CreateDefaultObjectValue(type);
                }

                if (!type.IsEnumType() && type.NamedType() is EnumType enumType)
                {
                    return enumType.Values.First().Name.ToString();
                }

                switch (type.NamedType().Name)
                {
                    case "String":
                        return "abc";
                    case "Int":
                        return 123;
                    case "Float":
                        return 123.123;
                    case "Boolean":
                        return true;
                    default:
                        throw new NotSupportedException();
                }
            }

            return null;
        }

        private static List<object?> CreateDefaultListValue(IType type) =>
            new() { CreateDefaultValue(type.ElementType()) };

        public static List<SchemaViolation> ValidateDictionary(
            Dictionary<string, object?> value,
            IType type)
        {
            var schemaViolations = new List<SchemaViolation>();
            ValidateDictionary(value, type, Path.Root, schemaViolations);
            return schemaViolations;
        }

        private static void ValidateDictionary(
            Dictionary<string, object?> value,
            IType type,
            Path path,
            List<SchemaViolation> schemaViolations)
        {
            if (!type.IsObjectType())
            {
                schemaViolations.Add(new SchemaViolation(path.ToList(), "NOT_AN_OBJECT"));
            }

            var objectType = (ObjectType)type.NamedType();

            foreach (var field in objectType.Fields)
            {
                if (field.IsIntrospectionField)
                {
                    continue;
                }

                if (value.TryGetValue(field.Name, out var fieldValue))
                {
                    Validate(fieldValue, field.Type, path, schemaViolations);
                }
                else if (field.Type.IsNonNullType())
                {
                    schemaViolations.Add(new SchemaViolation(
                        path.Append(field.Name).ToList(),
                        "NON_NULL"));
                }
            }

            foreach (var fieldName in value.Keys)
            {
                if (!objectType.Fields.ContainsField(fieldName))
                {
                    schemaViolations.Add(new SchemaViolation(
                        path.Append(fieldName).ToList(),
                        "UNKNOWN_FIELD"));
                }
            }
        }

        private static void Validate(
            object? value,
            IType type,
            Path path,
            List<SchemaViolation> schemaViolations)
        {
            switch (value)
            {
                case Dictionary<string, object?> dict:
                    ValidateDictionary(dict, type, path, schemaViolations);
                    break;

                case List<object?> list:
                    ValidateList(list, type, path, schemaViolations);
                    break;

                case string:
                    if (!type.IsScalarType() || type.NamedType() is not StringType)
                    {
                        schemaViolations.Add(new SchemaViolation(path.ToList(), "INVALID_TYPE"));
                    }
                    break;

                case int:
                    if (!type.IsScalarType() || type.NamedType() is not IntType and not FloatType)
                    {
                        schemaViolations.Add(new SchemaViolation(path.ToList(), "INVALID_TYPE"));
                    }
                    break;

                case bool:
                    if (!type.IsScalarType() || type.NamedType() is not BooleanType)
                    {
                        schemaViolations.Add(new SchemaViolation(path.ToList(), "INVALID_TYPE"));
                    }
                    break;

                case double:
                    if (!type.IsScalarType() || type.NamedType() is not FloatType)
                    {
                        schemaViolations.Add(new SchemaViolation(path.ToList(), "INVALID_TYPE"));
                    }
                    break;

                default:
                    schemaViolations.Add(new SchemaViolation(path.ToList(), "UNKNOWN_TYPE"));
                    break;
            }
        }

        private static void ValidateList(
            List<object?> value,
            IType type,
            Path path,
            List<SchemaViolation> schemaViolations)
        {
            IType elementType = type.ElementType();
            var i = 0;
            foreach (var element in value)
            {
                Validate(
                    element,
                    elementType,
                    path.Append(i++),
                    schemaViolations);
            }
        }

        public static Dictionary<string, object?> DeserializeDictionary(JsonElement element, IType type)
        {
            var dictionary = new Dictionary<string, object?>();
            var objectType = (ObjectType)type.NamedType();

            foreach (JsonProperty property in element.EnumerateObject())
            {
                IType fieldType = objectType.Fields[property.Name].Type;
                dictionary[property.Name] = Deserialize(property.Value, fieldType);
            }

            return dictionary;
        }

        private static object? Deserialize(JsonElement element, IType type)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    return DeserializeDictionary(element, type);

                case JsonValueKind.Array:
                    return DeserializeList(element, type);

                case JsonValueKind.String:
                    return element.GetString();

                case JsonValueKind.Number:
                    if (type.IsScalarType() && type.NamedType().Name.Equals(ScalarNames.Int))
                    {
                        return element.GetInt32();
                    }

                    return element.GetDouble();

                case JsonValueKind.True:
                    return true;

                case JsonValueKind.False:
                    return false;

                default:
                    return null;
            }
        }

        private static List<object?> DeserializeList(JsonElement array, IType type)
        {
            var list = new List<object?>();
            IType elementType = type.ElementType();

            foreach (JsonElement element in array.EnumerateArray())
            {
                list.Add(Deserialize(element, elementType));
            }

            return list;
        }
    }
}
