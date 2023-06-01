using System.Text.Json.Nodes;
using ConfiX.Variables;
using FluentAssertions;
using Xunit;
namespace Confix.Tool.Tests;

public class VariableProviderFactoryTests
{
    [Fact]
    public void CreateProvider_KnownProviderType_CreatesProvider()
    {
        // Arrange
        var factory = new VariableProviderFactory(new()
        {
            { LocalVariableProvider.PropertyType,(c) => new LocalVariableProvider(c) },
        });
        var providerType = LocalVariableProvider.PropertyType;
        var configuration = JsonNode.Parse("""
            {
                "path": "/path/to/file.json"
            }
            """)!;

        // Act
        var provider = factory.CreateProvider(providerType, configuration);

        // Assert
        provider.Should().NotBeNull();
        provider.Should().BeOfType<LocalVariableProvider>();
    }

    [Fact]
    public void CreateProvider_UnknownProviderType_ThrowsInvalidOperationException()
    {
        // Arrange
        var factory = new VariableProviderFactory(new()
        {
            { LocalVariableProvider.PropertyType,(c) => new LocalVariableProvider(c) },
        });
        var providerType = "UnknownProviderType";
        var configuration = JsonNode.Parse("""
            {
                "path": "/path/to/file.json"
            }
            """)!;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => factory.CreateProvider(providerType, configuration));
    }
}
