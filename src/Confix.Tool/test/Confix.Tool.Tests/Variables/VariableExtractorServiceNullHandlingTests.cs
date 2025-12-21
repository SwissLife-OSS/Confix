using System.Text.Json.Nodes;
using Confix.Variables;
using Moq;

namespace Confix.Tool.Tests;

/// <summary>
/// Tests for edge cases where JSON contains null values that could cause
/// null reference exceptions in <see cref="VariableExtractorService"/>.
/// These tests verify the fix for the warnings:
/// "Dereference of a possibly null reference" at line 64 in VariableExtractorService.cs
/// </summary>
public class VariableExtractorServiceNullHandlingTests
{
    [Fact]
    public async Task ExtractAsync_JsonObjectWithNullValue_DoesNotThrow()
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
        variableResolverMock.Setup(x => x.GetProviderType(It.IsAny<string>()))
            .Returns("test-provider");
        
        VariableExtractorService service = new(variableResolverMock.Object);

        // act & assert - should not throw NullReferenceException
        var result = await service.ExtractAsync(node, default);
        
        Assert.NotNull(result);
        Assert.Empty(result); // No variables in this JSON
    }

    [Fact]
    public async Task ExtractAsync_JsonObjectWithNestedNullValue_DoesNotThrow()
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
        variableResolverMock.Setup(x => x.GetProviderType(It.IsAny<string>()))
            .Returns("test-provider");
        
        VariableExtractorService service = new(variableResolverMock.Object);

        // act & assert - should not throw NullReferenceException
        var result = await service.ExtractAsync(node, default);
        
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ExtractAsync_JsonArrayWithNullElement_DoesNotThrow()
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
        variableResolverMock.Setup(x => x.GetProviderType(It.IsAny<string>()))
            .Returns("test-provider");
        
        VariableExtractorService service = new(variableResolverMock.Object);

        // act & assert - should not throw NullReferenceException
        var result = await service.ExtractAsync(node, default);
        
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ExtractAsync_JsonArrayWithOnlyNullElements_DoesNotThrow()
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
        variableResolverMock.Setup(x => x.GetProviderType(It.IsAny<string>()))
            .Returns("test-provider");
        
        VariableExtractorService service = new(variableResolverMock.Object);

        // act & assert - should not throw NullReferenceException
        var result = await service.ExtractAsync(node, default);
        
        Assert.NotNull(result);
        Assert.Empty(result); // No variables in null elements
    }

    [Fact]
    public async Task ExtractAsync_DeeplyNestedNullValues_DoesNotThrow()
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
        variableResolverMock.Setup(x => x.GetProviderType(It.IsAny<string>()))
            .Returns("test-provider");
        
        VariableExtractorService service = new(variableResolverMock.Object);

        // act & assert - should not throw NullReferenceException
        var result = await service.ExtractAsync(node, default);
        
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ExtractAsync_MixedNullAndVariables_ExtractsCorrectly()
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
        variableResolverMock.Setup(x => x.GetProviderType(It.IsAny<string>()))
            .Returns("test-provider");
        
        VariableExtractorService service = new(variableResolverMock.Object);

        // act
        var result = await service.ExtractAsync(node, default);

        // assert - should extract only the variables, not null values
        Assert.NotNull(result);
        var variables = result.ToList();
        Assert.Equal(2, variables.Count);
        Assert.Contains(variables, v => v.VariableName == "some.variable");
        Assert.Contains(variables, v => v.VariableName == "another.variable");
    }

    [Fact]
    public async Task ExtractAsync_ArrayWithMixedNullAndVariables_ExtractsCorrectly()
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
        variableResolverMock.Setup(x => x.GetProviderType(It.IsAny<string>()))
            .Returns("test-provider");
        
        VariableExtractorService service = new(variableResolverMock.Object);

        // act
        var result = await service.ExtractAsync(node, default);

        // assert
        Assert.NotNull(result);
        var variables = result.ToList();
        Assert.Equal(2, variables.Count);
        Assert.Contains(variables, v => v.VariableName == "variable.one");
        Assert.Contains(variables, v => v.VariableName == "variable.two");
    }

    [Fact]
    public async Task ExtractAsync_RootArrayWithNulls_DoesNotThrow()
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
        variableResolverMock.Setup(x => x.GetProviderType(It.IsAny<string>()))
            .Returns("test-provider");
        
        VariableExtractorService service = new(variableResolverMock.Object);

        // act & assert - should not throw NullReferenceException
        var result = await service.ExtractAsync(node, default);
        
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ExtractAsync_AllNullStructure_DoesNotThrow()
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
        variableResolverMock.Setup(x => x.GetProviderType(It.IsAny<string>()))
            .Returns("test-provider");
        
        VariableExtractorService service = new(variableResolverMock.Object);

        // act & assert - should not throw NullReferenceException
        var result = await service.ExtractAsync(node, default);
        
        Assert.NotNull(result);
        Assert.Empty(result); // No variables in null structure
    }

    [Fact]
    public async Task ExtractAsync_NullNode_ReturnsEmpty()
    {
        // arrange - null input
        JsonNode? node = null;

        Mock<IVariableResolver> variableResolverMock = new(MockBehavior.Strict);
        
        VariableExtractorService service = new(variableResolverMock.Object);

        // act
        var result = await service.ExtractAsync(node, default);

        // assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
