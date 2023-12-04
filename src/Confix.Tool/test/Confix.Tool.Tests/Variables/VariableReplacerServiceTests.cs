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
                    result[key] = JsonValue.Create("Replaced Value of " + key)!;
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
    public async Task RewriteAsync_JsonValue_()
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
}