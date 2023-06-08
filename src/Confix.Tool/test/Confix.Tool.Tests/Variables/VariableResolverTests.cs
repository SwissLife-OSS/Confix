using System.Text.Json.Nodes;
using ConfiX.Variables;
using FluentAssertions;
using Json.More;
using Moq;

namespace Confix.Tool.Tests;

public class VariableResolverTests
{
    [Fact]
    public async Task ResolveVariables_MultipleProviders_CorrectResult()
    {
        // Arrange
        var factoryMock = new Mock<IVariableProviderFactory>();


        var keys = new List<VariablePath>
        {
            new VariablePath("Provider1", "Key1"),
            new VariablePath("Provider2", "Key2"),
            new VariablePath("Provider1", "Key3"),
            new VariablePath("Provider2", "Key4")
        };

        var configurations = new List<VariableProviderConfiguration>
        {
            new VariableProviderConfiguration
            {
                Name = "Provider1",
                Type = "local",
                Configuration = JsonNode.Parse("""
                    {
                        "path": "/path/to/file.json"
                    }
                    """)!
            },
            new VariableProviderConfiguration
            {
                Name = "Provider2",
                Type = "local",
                Configuration = JsonNode.Parse("""
                    {
                        "path": "/path/to/file.json"
                    }
                    """)!
            }
        };
        var resolver = new VariableResolver(factoryMock.Object, configurations);
        var cancellationToken = CancellationToken.None;

        var provider1Mock = new Mock<IVariableProvider>();
        provider1Mock.Setup(p => p.ResolveManyAsync(
            It.Is<IReadOnlyList<string>>(paths => paths.SequenceEqual(new[] { "Key1", "Key3" })),
            cancellationToken))
            .ReturnsAsync(new Dictionary<string, JsonValue>
            {
                { "Key1", JsonValue.Create("Value1") },
                { "Key3", JsonValue.Create("Value3") }
            });

        var provider2Mock = new Mock<IVariableProvider>();
        provider2Mock.Setup(p => p.ResolveManyAsync(
            It.Is<IReadOnlyList<string>>(paths => paths.SequenceEqual(new[] { "Key2", "Key4" })),
            cancellationToken))
            .ReturnsAsync(new Dictionary<string, JsonValue>
            {
                { "Key2", JsonValue.Create("Value2") },
                { "Key4", JsonValue.Create("Value4") }
            });

        factoryMock.Setup(f => f.CreateProvider(configurations[0]))
            .Returns(provider1Mock.Object);

        factoryMock.Setup(f => f.CreateProvider(configurations[1]))
            .Returns(provider2Mock.Object);

        // Act
        var result = await resolver.ResolveVariables(keys, cancellationToken);

        // Assert
        result.Should().HaveCount(4);
        Assert.True(result[new VariablePath("Provider1", "Key1")].IsEquivalentTo(JsonValue.Create("Value1")));
        Assert.True(result[new VariablePath("Provider2", "Key2")].IsEquivalentTo(JsonValue.Create("Value2")));
        Assert.True(result[new VariablePath("Provider1", "Key3")].IsEquivalentTo(JsonValue.Create("Value3")));
        Assert.True(result[new VariablePath("Provider2", "Key4")].IsEquivalentTo(JsonValue.Create("Value4")));
    }

    [Fact]
    public async Task ResolveVariable_ProviderNotFound_ThrowsInvalidOperationExceptionAsync()
    {
        // Arrange
        var factoryMock = new Mock<IVariableProviderFactory>();
        var configurations = new List<VariableProviderConfiguration>();
        var resolver = new VariableResolver(factoryMock.Object, configurations);
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            resolver.ResolveVariable(new VariablePath("Provider1", "Key1"), cancellationToken));
    }

    [Fact]
    public async Task ResolveVariables_ProviderNotFound_ThrowsInvalidOperationExceptionAsync()
    {
        // Arrange
        var factoryMock = new Mock<IVariableProviderFactory>();

        var keys = new List<VariablePath>
        {
            new VariablePath("Provider1", "Key1")
        };

        var configurations = new List<VariableProviderConfiguration>();
        var resolver = new VariableResolver(factoryMock.Object, configurations);
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            resolver.ResolveVariables(keys, cancellationToken));
    }
}
