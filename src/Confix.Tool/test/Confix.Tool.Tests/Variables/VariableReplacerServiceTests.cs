using System.Text.Json.Nodes;
using Confix.Variables;
using Moq;
using Snapshooter.Xunit;

namespace Confix.Tool.Tests;

public class VariableReplacerServiceTests
{
    [Fact]
    public async Task RewriteAsync_ValidVariables_ReplaceAllVariablesAsync()
    {
        // arrange
        JsonNode node = JsonNode.Parse("""
            {
                "foo": {
                    "bar": "baz",
                    "test": "$test:variable.number",
                    "interpolated": "prefix-{{$test:variable.string1}}-suffix",
                    "interpolatedMultiple": "asterix-{{$test:variable.string2}}-midefix-{{$test:variable.string3}}-suffix"
                }
            }
        """)!;

        Mock<IVariableResolver> variableResolverMock = new(MockBehavior.Strict);
        variableResolverMock.Setup(x => x.ResolveVariables(
                It.IsAny<IReadOnlyList<VariablePath>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<VariablePath> keys, CancellationToken _) =>
            {
                var result = new Dictionary<VariablePath, JsonNode>();
                foreach (var key in keys)
                {
                    result[key] = JsonValue.Create("Replaced Value of " + key.Path)!;
                }
                return result;
            });
        VariableReplacerService service = new(variableResolverMock.Object);

        // act
        var result = await service.RewriteAsync(node, default);

        // assert
        result?.ToString().MatchSnapshot();
    }

    [Fact]
    public async Task RewriteAsync_JsonValue_ResolvesCorrectly()
    {
        // arrange
        JsonNode node = JsonValue.Create("$test:variable.number")!;

        Mock<IVariableResolver> variableResolverMock = new(MockBehavior.Strict);
        variableResolverMock.Setup(x => x.ResolveVariables(
                It.IsAny<IReadOnlyList<VariablePath>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<VariablePath> keys, CancellationToken _) =>
            {
                var result = new Dictionary<VariablePath, JsonNode>();
                foreach (var key in keys)
                {
                    result[key] = JsonValue.Create("Replaced")!;
                }
                return result;
            });
        VariableReplacerService service = new(variableResolverMock.Object);

        // act
        var result = await service.RewriteAsync(node, default);

        // assert
        Assert.Equal("Replaced", result?.ToString());
    }

    [Fact]
    public async Task RewriteAsync_VariablesInNestedArray_ResolvesCorrectly()
    {
        // arrange
        JsonNode node = JsonNode.Parse("""
            {
                "foo": "$test:variable.array"
            }
        """)!;

        Mock<IVariableResolver> variableResolverMock = new(MockBehavior.Strict);
        variableResolverMock.Setup(x => x.ResolveVariables(
                It.IsAny<IReadOnlyList<VariablePath>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<VariablePath> keys, CancellationToken _) =>
            {
                var result = new Dictionary<VariablePath, JsonNode>();
                foreach (var key in keys)
                {
                    if (key.Path == "variable.array")
                    {
                        result[key] = JsonNode.Parse("""
                            [
                                "notAVariable",
                                "$test:variable.string_nested"
                            ]
                        """)!;
                    }
                    else if(key.Path == "variable.string_nested"){
                        result[key] = JsonValue.Create("$test:variable.string")!;
                    }
                    else
                    {
                        result[key] = JsonValue.Create("Replaced Value of " + key.Path)!;
                    }
                }
                return result;
            });
        VariableReplacerService service = new(variableResolverMock.Object);

        // act
        var result = await service.RewriteAsync(node, default);

        // assert
        result?.ToString().MatchSnapshot();
    }

    [Fact]
    public async Task RewriteAsync_VariablesInNestedObject_ResolvesCorrectly()
    {
        // arrange
        JsonNode node = JsonNode.Parse("""
            {
                "foo": "$test:variable.someObject"
            }
        """)!;

        Mock<IVariableResolver> variableResolverMock = new(MockBehavior.Strict);
        variableResolverMock.Setup(x => x.ResolveVariables(
                It.IsAny<IReadOnlyList<VariablePath>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<VariablePath> keys, CancellationToken _) =>
            {
                var result = new Dictionary<VariablePath, JsonNode>();
                foreach (var key in keys)
                {
                    if (key.Path == "variable.someObject")
                    {
                        result[key] = JsonNode.Parse("""
                            {
                                "notAVariable": "notAVariable",
                                "string_nested": "$test:variable.string_nested"
                            }
                        """)!;
                    }
                    else if(key.Path == "variable.string_nested"){
                        result[key] = JsonValue.Create("$test:variable.string")!;
                    }
                    else
                    {
                        result[key] = JsonValue.Create("Replaced Value of " + key.Path)!;
                    }
                }
                return result;
            });
        VariableReplacerService service = new(variableResolverMock.Object);

        // act
        var result = await service.RewriteAsync(node, default);

        // assert
        result?.ToString().MatchSnapshot();
    }
}