using System.Text.Json.Nodes;
using Confix.Tool.Middlewares.Encryption.Providers.AzureKeyvault;
using FluentAssertions;

namespace Confix.Tool.Tests.Middlewares.Encryption;

public class AzureKeyVaultEncryptionProviderConfigurationTests
{
    [Fact]
    public void Parse_ValidConfiguration_ReturnsConfiguration()
    {
        // Arrange
        var json = JsonNode.Parse("""
        {
          "uri": "https://test.vault.azure.net/",
          "keyName": "test",
          "keyVersion": "test"
        }
        """)!;

        // Act
        var configuration = AzureKeyVaultEncryptionProviderConfiguration.Parse(json);

        // Assert
        configuration.Should().NotBeNull();
        configuration!.Uri.Should().Be("https://test.vault.azure.net/");
        configuration!.KeyName.Should().Be("test");
        configuration!.KeyVersion.Should().Be("test");
    }

    [Fact]
    public void Parse_EmptyObject_ParsesCorrectly()
    {
        // Arrange
        var json = JsonNode.Parse("{}")!;

        // Act
        var configuration = AzureKeyVaultEncryptionProviderConfiguration.Parse(json);

        // Assert
        configuration.Should().NotBeNull();
        configuration!.Uri.Should().BeNull();
        configuration!.KeyName.Should().BeNull();
        configuration!.KeyVersion.Should().BeNull();
    }
}
