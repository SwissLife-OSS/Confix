using System.Text.Json.Nodes;
using ConfiX.Variables;
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
                    "test": "$test:variable.number"
                }
            }
        """)!;

        Mock<IVariableResolver> variableResolverMock = new(MockBehavior.Strict);
        variableResolverMock.Setup(x => x.ResolveVariables(
                It.IsAny<IReadOnlyList<VariablePath>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<VariablePath> keys, CancellationToken _) =>
            {
                var result = new Dictionary<VariablePath, JsonValue>();
                foreach (var key in keys)
                {
                    result[key] = JsonValue.Create("Replaced Value of " + key);
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