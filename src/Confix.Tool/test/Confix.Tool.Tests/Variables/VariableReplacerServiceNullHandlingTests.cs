using System.Text.Json.Nodes;
using Confix.Variables;
using Moq;

namespace Confix.Tool.Tests;

/// <summary>
/// Tests for edge cases where JSON contains null values that could cause
/// null reference exceptions in <see cref="VariableReplacerService"/>.
/// These tests verify the fix for the warnings:
/// "Dereference of a possibly null reference" at line R48 in VariableReplacerService.cs
/// </summary>
public class VariableReplacerServiceNullHandlingTests
{
    [Fact]
    public async Task RewriteAsync_JsonObjectWithNullValue_DoesNotThrow()
    {
        // arrange - JSON with explicit null value
        JsonNode node = JsonNode.Parse("""
            {
                "foo": null,
                "bar": "baz"
            }
        """)!;

        Mock<IVariableResolver> variableResolverMock = new(MockBehavior.Strict);
        variableResolverMock.Setup(x => x.ResolveVariables(
                It.IsAny<IReadOnlyList<VariablePath>>(),
                It.IsAny<IVariableProviderContext>()))
            .ReturnsAsync(new Dictionary<VariablePath, JsonNode>());
        
        VariableReplacerService service = new(variableResolverMock.Object);

        // act & assert - should not throw NullReferenceException
        var result = await service.RewriteAsync(node, default);
        
        Assert.NotNull(result);
    }

    [Fact]
    public async Task RewriteAsync_JsonObjectWithNestedNullValue_DoesNotThrow()
    {
        // arrange - JSON with null value in nested object
        JsonNode node = JsonNode.Parse("""
            {
                "foo": {
                    "bar": null,
                    "baz": "value"
                }
            }
        """)!;

        Mock<IVariableResolver> variableResolverMock = new(MockBehavior.Strict);
        variableResolverMock.Setup(x => x.ResolveVariables(
                It.IsAny<IReadOnlyList<VariablePath>>(),
                It.IsAny<IVariableProviderContext>()))
            .ReturnsAsync(new Dictionary<VariablePath, JsonNode>());
        
        VariableReplacerService service = new(variableResolverMock.Object);

        // act & assert - should not throw NullReferenceException
        var result = await service.RewriteAsync(node, default);
        
        Assert.NotNull(result);
    }

    [Fact]
    public async Task RewriteAsync_JsonArrayWithNullElement_DoesNotThrow()
    {
        // arrange - JSON array with null element
        JsonNode node = JsonNode.Parse("""
            {
                "items": [
                    "value1",
                    null,
                    "value2"
                ]
            }
        """)!;

        Mock<IVariableResolver> variableResolverMock = new(MockBehavior.Strict);
        variableResolverMock.Setup(x => x.ResolveVariables(
                It.IsAny<IReadOnlyList<VariablePath>>(),
                It.IsAny<IVariableProviderContext>()))
            .ReturnsAsync(new Dictionary<VariablePath, JsonNode>());
        
        VariableReplacerService service = new(variableResolverMock.Object);

        // act & assert - should not throw NullReferenceException
        var result = await service.RewriteAsync(node, default);
        
        Assert.NotNull(result);
    }

    [Fact]
    public async Task RewriteAsync_JsonArrayWithOnlyNullElements_DoesNotThrow()
    {
        // arrange - JSON array with only null elements
        JsonNode node = JsonNode.Parse("""
            {
                "items": [null, null, null]
            }
        """)!;

        Mock<IVariableResolver> variableResolverMock = new(MockBehavior.Strict);
        variableResolverMock.Setup(x => x.ResolveVariables(
                It.IsAny<IReadOnlyList<VariablePath>>(),
                It.IsAny<IVariableProviderContext>()))
            .ReturnsAsync(new Dictionary<VariablePath, JsonNode>());
        
        VariableReplacerService service = new(variableResolverMock.Object);

        // act & assert - should not throw NullReferenceException
        var result = await service.RewriteAsync(node, default);
        
        Assert.NotNull(result);
    }

    [Fact]
    public async Task RewriteAsync_DeeplyNestedNullValues_DoesNotThrow()
    {
        // arrange - deeply nested structure with null values at various levels
        JsonNode node = JsonNode.Parse("""
            {
                "level1": {
                    "level2": {
                        "level3": null,
                        "items": [
                            null,
                            {
                                "nested": null
                            }
                        ]
                    },
                    "nullHere": null
                },
                "topLevelNull": null
            }
        """)!;

        Mock<IVariableResolver> variableResolverMock = new(MockBehavior.Strict);
        variableResolverMock.Setup(x => x.ResolveVariables(
                It.IsAny<IReadOnlyList<VariablePath>>(),
                It.IsAny<IVariableProviderContext>()))
            .ReturnsAsync(new Dictionary<VariablePath, JsonNode>());
        
        VariableReplacerService service = new(variableResolverMock.Object);

        // act & assert - should not throw NullReferenceException
        var result = await service.RewriteAsync(node, default);
        
        Assert.NotNull(result);
    }

    [Fact]
    public async Task RewriteAsync_MixedNullAndVariables_ResolvesCorrectly()
    {
        // arrange - JSON with both null values and variables
        JsonNode node = JsonNode.Parse("""
            {
                "nullValue": null,
                "variable": "$test:some.variable",
                "nested": {
                    "anotherNull": null,
                    "anotherVariable": "$test:another.variable"
                }
            }
        """)!;

        Mock<IVariableResolver> variableResolverMock = new(MockBehavior.Strict);
        variableResolverMock.Setup(x => x.ResolveVariables(
                It.IsAny<IReadOnlyList<VariablePath>>(),
                It.IsAny<IVariableProviderContext>()))
            .ReturnsAsync((IReadOnlyList<VariablePath> keys, IVariableProviderContext _) =>
            {
                var result = new Dictionary<VariablePath, JsonNode>();
                foreach (var key in keys)
                {
                    result[key] = JsonValue.Create("Resolved: " + key.Path)!;
                }
                return result;
            });
        
        VariableReplacerService service = new(variableResolverMock.Object);

        // act
        var result = await service.RewriteAsync(node, default);

        // assert - should resolve variables while preserving null values
        Assert.NotNull(result);
        var resultObj = result as JsonObject;
        Assert.NotNull(resultObj);
        
        // Null values should be preserved
        Assert.True(resultObj.ContainsKey("nullValue"));
        Assert.Null(resultObj["nullValue"]);
        
        // Variables should be resolved
        Assert.Equal("Resolved: some.variable", resultObj["variable"]?.ToString());
    }

    [Fact]
    public async Task RewriteAsync_ArrayWithMixedNullAndVariables_ResolvesCorrectly()
    {
        // arrange - JSON array with nulls and variables
        JsonNode node = JsonNode.Parse("""
            {
                "items": [
                    null,
                    "$test:variable.one",
                    null,
                    "$test:variable.two",
                    null
                ]
            }
        """)!;

        Mock<IVariableResolver> variableResolverMock = new(MockBehavior.Strict);
        variableResolverMock.Setup(x => x.ResolveVariables(
                It.IsAny<IReadOnlyList<VariablePath>>(),
                It.IsAny<IVariableProviderContext>()))
            .ReturnsAsync((IReadOnlyList<VariablePath> keys, IVariableProviderContext _) =>
            {
                var result = new Dictionary<VariablePath, JsonNode>();
                foreach (var key in keys)
                {
                    result[key] = JsonValue.Create("Resolved: " + key.Path)!;
                }
                return result;
            });
        
        VariableReplacerService service = new(variableResolverMock.Object);

        // act
        var result = await service.RewriteAsync(node, default);

        // assert
        Assert.NotNull(result);
        var resultObj = result as JsonObject;
        Assert.NotNull(resultObj);
        var items = resultObj["items"] as JsonArray;
        Assert.NotNull(items);
        
        // Check that nulls are preserved at correct positions
        Assert.Null(items[0]);
        Assert.Equal("Resolved: variable.one", items[1]?.ToString());
        Assert.Null(items[2]);
        Assert.Equal("Resolved: variable.two", items[3]?.ToString());
        Assert.Null(items[4]);
    }

    [Fact]
    public async Task RewriteAsync_RootArrayWithNulls_DoesNotThrow()
    {
        // arrange - Root level array with null elements
        JsonNode node = JsonNode.Parse("""
            [
                null,
                "string",
                null,
                {"key": "value"},
                null
            ]
        """)!;

        Mock<IVariableResolver> variableResolverMock = new(MockBehavior.Strict);
        variableResolverMock.Setup(x => x.ResolveVariables(
                It.IsAny<IReadOnlyList<VariablePath>>(),
                It.IsAny<IVariableProviderContext>()))
            .ReturnsAsync(new Dictionary<VariablePath, JsonNode>());
        
        VariableReplacerService service = new(variableResolverMock.Object);

        // act & assert - should not throw NullReferenceException
        var result = await service.RewriteAsync(node, default);
        
        Assert.NotNull(result);
    }

    [Fact]
    public async Task RewriteAsync_AllNullStructure_DoesNotThrow()
    {
        // arrange - Structure where all leaf values are null
        JsonNode node = JsonNode.Parse("""
            {
                "a": null,
                "b": {
                    "c": null
                },
                "d": [null]
            }
        """)!;

        Mock<IVariableResolver> variableResolverMock = new(MockBehavior.Strict);
        variableResolverMock.Setup(x => x.ResolveVariables(
                It.IsAny<IReadOnlyList<VariablePath>>(),
                It.IsAny<IVariableProviderContext>()))
            .ReturnsAsync(new Dictionary<VariablePath, JsonNode>());
        
        VariableReplacerService service = new(variableResolverMock.Object);

        // act & assert - should not throw NullReferenceException
        var result = await service.RewriteAsync(node, default);
        
        Assert.NotNull(result);
    }
}
