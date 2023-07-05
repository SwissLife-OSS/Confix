using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using FluentAssertions;
using Snapshooter.Xunit;

namespace Confix.Tool.Tests.Entities.Encryption;

public class EncryptionDefinitionTests
{
    [Fact]
    public void From_ProviderNull_ThrowsValidationException()
    {
        // Arrange
        var configuration = new EncryptionConfiguration(null);

        // Act
        var exception = Assert.Throws<ValidationException>(() => EncryptionDefinition.From(configuration));

        // Assert
        exception.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void From_ValidProvider_ReturnsDefinition()
    {
        // Arrange
        var configuration = new EncryptionConfiguration(
            new EncryptionProviderConfiguration(
                "test", 
                null, 
                JsonNode.Parse("{}")!.AsObject()));

        // Act
        var definition = EncryptionDefinition.From(configuration);

        // Assert
        definition.MatchSnapshot();
    }
}
