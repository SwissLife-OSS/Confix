using Confix.Tool.Middlewares.Encryption.Providers.AzureKeyvault;
using FluentAssertions;

namespace Confix.Tool.Tests.Middlewares.Encryption;

public class AzureKeyVaultEncryptionProviderDefinitionTests
{
    [Fact]
    public void From_ValidConfiguration_ReturnsDefinition()
    {
        // Arrange
        var configuration = new AzureKeyVaultEncryptionProviderConfiguration(
            "https://test.vault.azure.net/",
            "test",
            "test");

        // Act
        var definition = AzureKeyVaultEncryptionProviderDefinition.From(configuration);

        // Assert
        definition.Should().NotBeNull();
        definition!.Uri.Should().Be("https://test.vault.azure.net/");
        definition!.KeyName.Should().Be("test");
        definition!.KeyVersion.Should().Be("test");
    }

    [Fact]
    public void From_InvalidConfiguration_ThrowsException()
    {
        // Arrange
        var configuration = new AzureKeyVaultEncryptionProviderConfiguration(null, null, null);

        // Act
        var exception = Assert.Throws<ValidationException>(() => AzureKeyVaultEncryptionProviderDefinition.From(configuration));

        // Assert
        exception.Errors.Should().HaveCount(2);
    }
}