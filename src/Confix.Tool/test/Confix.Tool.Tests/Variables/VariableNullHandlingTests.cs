using System.Text.Json.Nodes;
using Confix.Variables;
using Moq;

namespace Confix.Tool.Tests;

public class VariableNullHandlingTests
{
    [Fact]
    public async Task ExtractAsync_MixedNullsAndVariable_ExtractsVariable()
    {
        var node = JsonNode.Parse(
            """
            {
              "nullValue": null,
              "nested": [null, "$test:value"]
            }
            """)!;
        var path = VariablePath.Parse("$test:value");
        var resolver = new Mock<IVariableResolver>(MockBehavior.Strict);
        resolver
            .Setup(x => x.ResolveVariables(
                It.Is<IReadOnlyList<VariablePath>>(paths => paths.SequenceEqual(new[] { path })),
                It.IsAny<IVariableProviderContext>()))
            .ReturnsAsync(new Dictionary<VariablePath, JsonNode>
            {
                [path] = JsonValue.Create("resolved")!
            });
        resolver.Setup(x => x.GetProviderType("test")).Returns("test-provider");
        var service = new VariableExtractorService(resolver.Object);
        var context = Mock.Of<IVariableProviderContext>();

        var result = (await service.ExtractAsync(node, context)).ToArray();

        var variable = Assert.Single(result);
        Assert.Equal("value", variable.VariableName);
        Assert.Equal("resolved", variable.VariableValue);
    }

    [Fact]
    public async Task RewriteAsync_MixedNullsAndVariable_PreservesNullsAndResolvesVariable()
    {
        var node = JsonNode.Parse(
            """
            {
              "nullValue": null,
              "nested": [null, "$test:value"]
            }
            """)!;
        var path = VariablePath.Parse("$test:value");
        var resolver = new Mock<IVariableResolver>(MockBehavior.Strict);
        resolver
            .Setup(x => x.ResolveVariables(
                It.Is<IReadOnlyList<VariablePath>>(paths => paths.SequenceEqual(new[] { path })),
                It.IsAny<IVariableProviderContext>()))
            .ReturnsAsync(new Dictionary<VariablePath, JsonNode>
            {
                [path] = JsonValue.Create("resolved")!
            });
        var service = new VariableReplacerService(resolver.Object);
        var context = Mock.Of<IVariableProviderContext>();

        var result = Assert.IsType<JsonObject>(await service.RewriteAsync(node, context));

        Assert.Null(result["nullValue"]);
        var nested = Assert.IsType<JsonArray>(result["nested"]);
        Assert.Null(nested[0]);
        Assert.Equal("resolved", nested[1]!.GetValue<string>());
    }
}
