using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Entities.Components.DotNet;
using Confix.Tool.Schema;
using ConfiX.Variables;
using Json.Schema;
using Snapshooter.Xunit;

namespace ConfiX;

public class DefaultValueVisitorTests
{
    [Fact]
    public void Should_InitializeFields_When_Required()
    {
        // arrange
        var schema = Build("""
            type Root {
                required: String!
                notRequired: String
            }
        """);

        var value = JsonNode.Parse("""
            { }
        """);

        // act
        var result = DefaultValueVisitor.ApplyDefaults(schema, value!);

        // assert
        result.MatchInline("""
            {
              "required": null
            }
            """);
    }

    [Fact]
    public void Should_InitializeObjects_When_FieldsAreRequired()
    {
        // arrange
        var schema = Build("""
            type Root {
                deep: Deep!
            }
            type Deep {
                required: String!
                notRequired: String
            }
        """);

        var value = JsonNode.Parse("""
            { }
        """);

        // act
        var result = DefaultValueVisitor.ApplyDefaults(schema, value!);

        // assert
        result.MatchInline("""
            {
              "deep": {
                "required": null
              }
            }
            """);
    }

    [Fact]
    public void Should_InitializeArrays_When_Required()
    {
        // arrange
        var schema = Build("""
            type Root {
                required: [String]!
                notRequired: [String]
            }
        """);

        var value = JsonNode.Parse("""
            { }
        """);

        // act
        var result = DefaultValueVisitor.ApplyDefaults(schema, value!);

        // assert
        result.MatchInline("""
            {
              "required": []
            }
            """);
    }

    [Fact]
    public void Should_InitializeArraysOfObjects_When_FieldsAreRequired()
    {
        // arrange
        var schema = Build("""
            type Root {
                deep: [Deep!]!
            }
            type Deep {
                required: String!
                notRequired: String
            }
        """);

        var value = JsonNode.Parse("""
            { }
        """);

        // act
        var result = DefaultValueVisitor.ApplyDefaults(schema, value!);

        // assert
        result.MatchInline("""
            {
              "deep": []
            }
            """);
    }

    [Fact]
    public void Should_InitializeArraysOfArrays_When_FieldsAreRequired()
    {
        // arrange
        var schema = Build("""
            type Root {
                deep: [[String!]!]!
            }
        """);

        var value = JsonNode.Parse("""
            { }
        """);

        // act
        var result = DefaultValueVisitor.ApplyDefaults(schema, value!);

        // assert
        result.MatchInline("""
            {
              "deep": []
            }
            """);
    }

    [Fact]
    public void Should_InitializeArraysOfArraysOfObjects_When_FieldsAreRequired()
    {
        // arrange
        var schema = Build("""
            type Root {
                deep: [[Deep!]!]!
            }
            type Deep {
                required: String!
                notRequired: String
            }
        """);

        var value = JsonNode.Parse("""
            { }
        """);

        // act
        var result = DefaultValueVisitor.ApplyDefaults(schema, value!);

        // assert
        result.MatchInline("""
            {
              "deep": []
            }
            """);
    }

    [Fact]
    public void Should_AddDefaultValues_When_FieldIsNotSpecifiedAndHasDefaultValue()
    {
        // arrange
        var schema = Build("""
            type Root {
                visible: String @defaultValue(value: "visible")
                notVisible: String
            }
        """);

        var value = JsonNode.Parse("""
            { }
        """);

        // act
        var result = DefaultValueVisitor.ApplyDefaults(schema, value!);

        // assert
        result.MatchInline("""
            {
              "visible": "visible"
            }
            """);
    }

    [Fact]
    public void Should_NotAddDefaultValue_When_FieldIsNotRequiredButSetToNull()
    {
        // arrange
        var schema = Build("""
            type Root {
                visible: String @defaultValue(value: "visible")
                notVisible: String
            }
        """);

        var value = JsonNode.Parse("""
            { 
                "visible": null
            }
        """);

        // act
        var result = DefaultValueVisitor.ApplyDefaults(schema, value!);

        // assert
        result.MatchInline("""
            {
              "visible": null
            }
            """);
    }

    [Fact]
    public void Should_NotAddDefaultValue_When_FieldIsAlreadySpecified()
    {
        // arrange
        var schema = Build("""
            type Root {
                visible: String @defaultValue(value: "visible")
                notVisible: String
            }
        """);

        var value = JsonNode.Parse("""
            { 
                "visible": ""
            }
        """);

        // act
        var result = DefaultValueVisitor.ApplyDefaults(schema, value!);

        // assert
        result.MatchInline("""
            {
              "visible": ""
            }
            """);
    }

    [Fact]
    public void Should_AddDefaultValuesToObject_When_ObjectIsRequired()
    {
        // arrange
        var schema = Build("""
            type Root {
                deep: Deep!
            }
            type Deep {
                visible: String @defaultValue(value: "visible")
                notVisible: String
            }
        """);

        var value = JsonNode.Parse("""
            { }
        """);

        // act
        var result = DefaultValueVisitor.ApplyDefaults(schema, value!);

        // assert
        result.MatchInline("""
            {
              "deep": {
                "visible": "visible"
              }
            }
            """);
    }

    [Fact]
    public void Should_NotAddDefaultValuesToObject_When_ObjectIsNotRequired()
    {
        // arrange
        var schema = Build("""
            type Root {
                deep: Deep
            }
            type Deep {
                visible: String @defaultValue(value: "visible")
                notVisible: String
            }
        """);

        var value = JsonNode.Parse("""
            { }
        """);

        // act
        var result = DefaultValueVisitor.ApplyDefaults(schema, value!);

        // assert
        result.MatchInline("""
            {}
            """);
    }

    [Fact]
    public void Should_AddDefaultValue_When_OtherFieldIsRequired()
    {
        // arrange
        var schema = Build("""
            type Root {
                deep: Deep!
            }

            type Deep {
                visible: String @defaultValue(value: "visible")
                required: String!
            }
        """);

        var value = JsonNode.Parse("""
            { }
        """);

        // act
        var result = DefaultValueVisitor.ApplyDefaults(schema, value!);

        // assert
        result.MatchInline("""
            {
              "deep": {
                "visible": "visible",
                "required": null
              }
            }
            """);
    }

    [Fact]
    public void Should_NotRemoveOtherFields_When_FieldsArePresent()
    {
        // arrange
        var schema = Build("""
            type Root {
                visible: String @defaultValue(value: "visible")
                notVisible: String
            }
        """);

        var value = JsonNode.Parse("""
            { 
                "otherField": "otherValue"
            }
        """);

        // act
        var result = DefaultValueVisitor.ApplyDefaults(schema, value!);

        // assert
        result.MatchInline("""
            {
              "otherField": "otherValue",
              "visible": "visible"
            }
            """);
    }

    [Fact]
    public void Should_InitializeFields_When_PartOfTreeIsAlreadyThere()
    {
        // arrange
        var schema = Build("""
            type Root {
                deep: Deep!
            }
            type Deep {
                alreadyDefined: String
                required: String!
                deeper: Deeper!
            }
            type Deeper {
                visible: String @defaultValue(value: "visible")
                notVisible: String
            }
        """);

        var value = JsonNode.Parse("""
            { 
                "deep": {
                    "alreadyDefined": "defined"
                }
            }
        """);

        // act
        var result = DefaultValueVisitor.ApplyDefaults(schema, value!);

        // assert
        result.MatchInline("""
            {
              "deep": {
                "alreadyDefined": "defined",
                "required": null,
                "deeper": {
                  "visible": "visible"
                }
              }
            }
            """);
    }

    [Fact]
    public void Should_Initialize_PartialObjectsInLists_When_TheyHaveRequiredProperties()
    {
        // arrange
        var schema = Build("""
            type Root {
                nested: [Nested!]!
            }
            type Nested { 
                required: String!
                notRequired: String
            }

        """);

        var value = JsonNode.Parse("""
            { 
              "nested": [
                  {}
              ]
            }
        """);

        // act
        var result = DefaultValueVisitor.ApplyDefaults(schema, value!);

        // assert
        result.MatchInline("""
            {
              "nested": [
                {
                  "required": null
                }
              ]
            }
            """);
    }

    [Fact]
    public void Should_Initialize_PartialRequiredObjects()
    {
        // arrange
        var schema = Build("""
            type Root {
                nested: Nested! @defaultValue(value: { deep: {required: "thisIsSetFromRoot"} })
            }
            type Nested { 
                deep: Deep!
                notRequired: String
                required: String!
                nonRequiredWithDefault: String @defaultValue(value: "default")
                requiredWithDefault: String! @defaultValue(value: "default")
            }
            type Deep {
                notRequired: String
                required: String!
                nonRequiredWithDefault: String @defaultValue(value: "default")
                requiredWithDefault: String! @defaultValue(value: "default")
            }
        """);

        var value = JsonNode.Parse("""
            { 
            }
        """);

        // act
        var result = DefaultValueVisitor.ApplyDefaults(schema, value!);

        // assert
        result.MatchInline("""
            {
              "nested": {
                "deep": {
                  "required": "thisIsSetFromRoot",
                  "nonRequiredWithDefault": "default",
                  "requiredWithDefault": "default"
                },
                "required": null,
                "nonRequiredWithDefault": "default",
                "requiredWithDefault": "default"
              }
            }
            """);
    }

    [Fact]
    public void Should_Initialize_Should_NotOverrideParentDefaultValues()
    {
        // arrange
        var schema = Build("""
            type Root {
                nested: Nested! @defaultValue(value: 
                        { 
                            requiredWithDefault: "thisIsSetFromRoot"
                            deep: {
                                requiredWithDefault: "thisIsSetFromRoot"
                            } 
                        })
            }
            type Nested { 
                deep: Deep!
                notRequired: String
                required: String!
                nonRequiredWithDefault: String @defaultValue(value: "default")
                requiredWithDefault: String! @defaultValue(value: "default")
            }
            type Deep {
                notRequired: String
                required: String!
                nonRequiredWithDefault: String @defaultValue(value: "default")
                requiredWithDefault: String! @defaultValue(value: "default")
            }
        """);

        var value = JsonNode.Parse("""
            { 
            }
        """);

        // act
        var result = DefaultValueVisitor.ApplyDefaults(schema, value!);

        // assert
        result.MatchInline("""
            {
              "nested": {
                "requiredWithDefault": "thisIsSetFromRoot",
                "deep": {
                  "requiredWithDefault": "thisIsSetFromRoot",
                  "required": null,
                  "nonRequiredWithDefault": "default"
                },
                "required": null,
                "nonRequiredWithDefault": "default"
              }
            }
            """);
    }

    [Fact]
    public void Should_DoNothing_When_PropertyIsThereWithInvalidType()
    {
        // arrange
        var schema = Build("""
            type Root {
                nested: Nested! @defaultValue(value: {fail: "fail"})
            }
            type Nested {
                notRequired: String
                required: String!
            }
        """);

        var value = JsonNode.Parse("""
            {
              "nested": "should-still-exists"
            }
        """);

        // act
        var result = DefaultValueVisitor.ApplyDefaults(schema, value!);

        // assert
        result.MatchInline("""
            {
              "nested": "should-still-exists"
            }
            """);
    }

    [Fact]
    public void Should_NotSetDefaultValue_When_ValueIsAlreadyPresent()
    {
        // arrange
        var schema = Build("""
            type Root {
                nested: Nested! @defaultValue(value: {fail: "fail"})
                wrongType: String @defaultValue(value: 1)
                required: String! @defaultValue(value: "default")
                array: [String]! @defaultValue(value: ["default"])
            }
            type Nested {
                notRequired: String
                required: String!
            }
        """);

        var value = JsonNode.Parse("""
            {
              "nested": null,
              "wrongType": null,
              "required": null,
              "array": null
            }
        """);

        // act
        var result = DefaultValueVisitor.ApplyDefaults(schema, value!);

        // assert
        result.MatchInline("""
            {
              "nested": {
                "required": null
              },
              "wrongType": null,
              "required": null,
              "array": null
            }
            """);
    }

    [Fact]
    public void ComposedSchema_Should_InitializeRequiredFields()
    {
        // arrange
        var jsonSchema = Create(
            new Definition("Foo", """type Query { foo: String! }"""),
            new Definition("Bar", """type Query { bar: String! }"""));

        // act
        var result = DefaultValueVisitor.ApplyDefaults(jsonSchema, JsonNode.Parse("{}")!);

        // assert
        Match(result);
    }

    [Fact]
    public void ComposedSchema_Should_InitializeAWholeTree()
    {
        // arrange
        var jsonSchema = Create(
            new Definition("Foo",
                """
                type Query { 
                    deep: Deep! 
                }
                type Deep { 
                    required: String!
                    notRequired: String
                    deeper: Deeper! 
                }
                type Deeper { 
                    required: String!
                    notRequired: String
                    nonRequired: NonRequired
                }
                type NonRequired { 
                    required: String!
                    notRequired: String
                }
                """));

        // act
        var result = DefaultValueVisitor.ApplyDefaults(jsonSchema, JsonNode.Parse("{}")!);

        // assert
        Match(result);
    }

    [Fact]
    public void ComposedSchema_Should_InitializeWithArray()
    {
        // arrange
        var jsonSchema = Create(
            new Definition("Foo",
                """
                type Query { 
                    deep: Deep! 
                }
                type Deep { 
                    required: [String]!
                    notRequired: [String]
                    deeper: [Deeper]! 
                }
                type Deeper { 
                    required: String!
                    notRequired: String
                }
                """));

        // act
        var result = DefaultValueVisitor.ApplyDefaults(jsonSchema, JsonNode.Parse("{}")!);

        // assert
        Match(result);
    }

    [Fact]
    public void ComposedSchema_Should_Complete_When_RecursiveRequiredProperties()
    {
        // arrange
        var jsonSchema = Create(
            new Definition("Foo",
                """
                type Query { 
                    deep: Deep! 
                }
                type Deep { 
                    deeper: Deeper! 
                }
                type Deeper { 
                    deep: Deep!
                }
                """));

        // act
        var result = DefaultValueVisitor.ApplyDefaults(jsonSchema, JsonNode.Parse("{}")!);

        // assert
        Match(result);
    }

    [Fact]
    public void ComposedSchema_Should_Complete_When_RecursiveRequiredProperties_With_Array()
    {
        // arrange
        var jsonSchema = Create(
            new Definition("Foo",
                """
                type Query { 
                    deep: Deep! 
                }
                type Deep { 
                    deeper: [Deeper]! 
                }
                type Deeper { 
                    deep: Deep!
                }
                """));

        // act
        var result = DefaultValueVisitor.ApplyDefaults(jsonSchema, JsonNode.Parse("{}")!);

        // assert
        Match(result);
    }

    [Fact]
    public void ComposedSchema_Should_SetDefaultValue()
    {
        // arrange
        var jsonSchema = Create(
            new Definition("Foo",
                """type Query { required: String! @defaultValue(value: "bar") }"""),
            new Definition("Bar",
                """type Query { nonRequired: String @defaultValue(value: "bar") }"""));

        // act
        var result = DefaultValueVisitor.ApplyDefaults(jsonSchema, JsonNode.Parse("{}")!);

        // assert
        Match(result);
    }

    [Fact]
    public void ComposedSchema_Should_NotSetDefaultValue_When_RootIsSet()
    {
        // arrange
        var jsonSchema = Create(
            new Definition("Bar",
                """type Query { nonRequired: String @defaultValue(value: "bar") }"""));

        // act
        var result =
            DefaultValueVisitor.ApplyDefaults(jsonSchema, JsonNode.Parse(""" {"Bar": null} """)!);

        // assert
        Match(result);
    }

    [Fact]
    public void ComposedSchema_Should_SetDefaultValue_Deep()
    {
        // arrange
        var jsonSchema = Create(
            new Definition("Foo",
                """
                type Query { 
                    deep: Deep! 
                }
                type Deep { 
                    required: String!
                    deeper: Deeper! 
                }
                type Deeper { 
                    required: String! @defaultValue(value: "Foo")
                }
                """));

        // act
        var result = DefaultValueVisitor.ApplyDefaults(jsonSchema, JsonNode.Parse("{}")!);

        // assert
        Match(result);
    }

    [Fact]
    public void ComposedSchema_Should_SetDefaultValue_ShouldNotInitializeNonRequired()
    {
        // arrange
        var jsonSchema = Create(
            new Definition("Foo",
                """
                type Query { 
                    deep: Deep 
                }
                type Deep { 
                    required: String!
                }
                """),
            new Definition("Bar",
                """
                type Query { 
                    deep: Deep! 
                }
                type Deep { 
                    required: String!
                    deeper: Deeper
                }
                type Deeper { 
                    required: String! @defaultValue(value: "Foo")
                }
                """),
            new Definition("Baz",
                """
                type Query { 
                    deep: Deep! 
                }
                type Deep { 
                    deeper: Deeper
                }
                type Deeper { 
                    required: String! @defaultValue(value: "Foo")
                }
                """));

        // act
        var result = DefaultValueVisitor.ApplyDefaults(jsonSchema, JsonNode.Parse("{}")!);

        // assert
        Match(result);
    }

    [Fact]
    public void Should_InitializeComplexObjectArrays_When_InComponent()
    {
        // arrange

        var jsonSchema = Create(
            new Definition("HealthChecks",
                """
                type Configuration {
                  Endpoints: [Endpoint!]! @defaultValue(value: [
                    {
                      Path: "/_health/live",
                      TagsPredicate: [
                        "liveness"
                      ]
                    },
                    {
                      Path: "/_health/ready",
                      TagsPredicate: [
                        "readyness"
                      ]
                    }
                  ])
                }

                type Endpoint {
                  Path: String!
                  TagsPredicate: [String]!
                }
                """));
        // act
        var result = DefaultValueVisitor.ApplyDefaults(jsonSchema, JsonNode.Parse("{}")!);

        // assert
        result.MatchInline("""
            {
              "HealthChecks": {
                "Endpoints": [
                  {
                    "Path": "/_health/live",
                    "TagsPredicate": [
                      "liveness"
                    ]
                  },
                  {
                    "Path": "/_health/ready",
                    "TagsPredicate": [
                      "readyness"
                    ]
                  }
                ]
              }
            }
            """);
    }

    private static void Match(JsonNode result)
    {
        result.ToJsonString(new JsonSerializerOptions { WriteIndented = true }).MatchSnapshot();
    }

    private static JsonSchema Build(string schema)
        => SchemaHelpers.BuildSchema(schema).ToJsonSchema();

    private static JsonSchema Create(params Definition[] definitions)
    {
        var components = definitions
            .Select(x =>
            {
                var schema = Build(x.Schema);
                return new Component(x.Name, x.Name, null, true, new[] { x.Name }, schema);
            })
            .ToArray();
        var variables = Array.Empty<VariablePath>();

        var composer = new ProjectComposer();
        return composer.Compose(components, variables);
    }

    private record Definition(string Name, string Schema);
}

file static class Extensions
{
    public static void MatchInline(this JsonNode result, string expected)
    {
        var actual = result.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        if (!expected.Equals(actual))
        {
            throw new Xunit.Sdk.XunitException(
                $"Expected: {Environment.NewLine}{expected}{Environment.NewLine}Actual: {Environment.NewLine}{actual}");
        }
    }
}
