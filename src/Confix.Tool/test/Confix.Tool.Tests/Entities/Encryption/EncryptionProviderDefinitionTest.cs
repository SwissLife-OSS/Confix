using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using FluentAssertions;
using Snapshooter.Xunit;

namespace Confix.Tool.Tests.Entities.Encryption;

public class EncryptionProviderDefinitionTest
{
    [Fact]
    public void From_TypeNull_ThrowsValidationException()
    {
        // Arrange
        var configuration = new EncryptionProviderConfiguration(null, null, JsonNode.Parse("{}")!.AsObject());

        // Act
        var exception = Assert.Throws<ValidationException>(() => EncryptionProviderDefinition.From(configuration));

        // Assert
        exception.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void From_ValidProvider_ReturnsDefinition()
    {
        // Arrange
        var configuration = new EncryptionProviderConfiguration(
            "test",
            null,
            JsonNode.Parse("{}")!.AsObject());

        // Act
        var definition = EncryptionProviderDefinition.From(configuration);

        // Assert
        definition.MatchSnapshot();
    }

    [Fact]
    public void ValueWithOverrides_EnvironmentOverride_ReturnsMergedValue()
    {
        // Arrange
        var configuration = new EncryptionProviderConfiguration(
            "test",
            new Dictionary<string, JsonObject>
            {
                { "test", JsonNode.Parse("""{ "test": "override" }""")!.AsObject() }
            },
            JsonNode.Parse("""{ "test": "test" }""")!.AsObject());
        var definition = EncryptionProviderDefinition.From(configuration);

        // Act
        var value = definition.ValueWithOverrides("test");

        // Assert
        value.ToJsonString(new() { WriteIndented = true }).MatchSnapshot();
    }
}