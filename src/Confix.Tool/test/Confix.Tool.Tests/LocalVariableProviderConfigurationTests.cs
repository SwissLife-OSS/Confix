using System.Text.Json.Nodes;
using ConfiX.Variables;
using FluentAssertions;
using Xunit;
namespace Confix.Tool.Tests;

public class LocalVariableProviderConfigurationTests
{
    [Fact]
    public void Parse_ValidJsonNode_CorrectResult()
    {
        // Arrange
        var jsonNode = JsonNode.Parse("""
            {
                "path": "/path/to/file.json"
            }
            """)!;

        // Act
        var config = LocalVariableProviderConfiguration.Parse(jsonNode);

        // Assert
        config.FilePath.Should().Be("/path/to/file.json");
    }

    [Fact]
    public void Parse_InvalidJsonNode_MissingProperty_ThrowsArgumentException()
    {
        // Arrange
        var jsonNode = JsonNode.Parse("""
            {
                "some": "property"
            }
            """)!;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => LocalVariableProviderConfiguration.Parse(jsonNode));
    }
}
