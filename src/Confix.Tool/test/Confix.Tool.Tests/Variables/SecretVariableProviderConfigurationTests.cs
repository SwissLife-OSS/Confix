using System.Text.Json.Nodes;
using ConfiX.Variables;
using FluentAssertions;
using Snapshooter.Xunit;

namespace Confix.Tool.Tests;

public class SecretVariableProviderConfigurationTests
{

    [Fact]
    public void Parse_EmptyObject_Valid()
    {
        // arrange
        var configuration = JsonNode.Parse("{}")!;

        // act
        var result = SecretVariableProviderConfiguration.Parse(configuration);

        // assert
        result.Should().MatchSnapshot();
    }

    [Fact]
    public void Parse_WithValidConfigurationKey_ReturnsValidObject()
    {
        // arrange
        var configuration = JsonNode.Parse(
            """
            {
                "algorithm": "RSA",
                "padding": "OaepSHA256",
                "publicKey": "SomePublicKey",
                "privateKey": "SomePrivateKey"
            }
            """
        )!;

        // act
        var result = SecretVariableProviderConfiguration.Parse(configuration);

        // assert
        result.Should().MatchSnapshot();
    }

    [Fact]
    public void Parse_WithValidConfigurationPath_ReturnsValidObject()
    {
        // arrange
        var configuration = JsonNode.Parse(
            """
            {
                "algorithm": "RSA",
                "publicKeyPath": "./pub.pem",
                "privateKeyPath": "./priv.pem"
            }
            """
        )!;

        // act
        var result = SecretVariableProviderConfiguration.Parse(configuration);

        // assert
        result.Should().MatchSnapshot();
    }
}

public class SecretVariableProviderDefinitionTests
{
    [Fact]
    public void From_Should_Be_Valid_When_PublicKeyAndPrivateKeyAreSpecified()
    {
        // Arrange
        var configuration = new SecretVariableProviderConfiguration(
            SecretVariableProviderAlgorithm.RSA,
            EncryptionPadding.OaepSHA256,
            "PublicKey",
            null,
            "PrivateKey",
            null,
            null);

        // Act
        var result = SecretVariableProviderDefinition.From(configuration);

        // Assert
        result.Should().MatchSnapshot();
    }

    [Fact]
    public void From_Should_Throw_When_AlgorithmIsNull()
    {
        // Arrange
        var configuration = new SecretVariableProviderConfiguration(
            null,
            EncryptionPadding.OaepSHA256,
            "PublicKey",
            null,
            "PrivateKey",
            null,
            null);

        // Act
        var exception = Assert.Throws<ValidationException>(() => SecretVariableProviderDefinition.From(configuration));

        // Assert
        exception.Errors.Should().HaveCount(1);
        exception.Errors.Should().Contain("Algorithm is required.");
    }

    [Fact]
    public void From_Should_Throw_When_PaddingIsNull()
    {
        // Arrange
        var configuration = new SecretVariableProviderConfiguration(
            SecretVariableProviderAlgorithm.RSA,
            null,
            "PublicKey",
            null,
            "PrivateKey",
            null,
            null);

        // Act
        var exception = Assert.Throws<ValidationException>(() => SecretVariableProviderDefinition.From(configuration));

        // Assert
        exception.Errors.Should().HaveCount(1);
        exception.Errors.Should().Contain("Padding is required.");
    }

    [Fact]
    public void From_Should_Throw_When_PublicKeyAndPublicKeyPathAreSpecified()
    {
        // Arrange
        var configuration = new SecretVariableProviderConfiguration(
            SecretVariableProviderAlgorithm.RSA,
            EncryptionPadding.OaepSHA256,
            "PublicKey",
            "PublicKeyPath",
            "PrivateKey",
            null,
            null);

        // Act
        var exception = Assert.Throws<ValidationException>(() => SecretVariableProviderDefinition.From(configuration));

        // Assert
        exception.Errors.Should().HaveCount(1);
        exception.Errors.Should().Contain("PublicKey and PublicKeyPath cannot be specified at the same time.");
    }

    [Fact]
    public void From_Should_Throw_When_PrivateKeyAndPrivateKeyPathAreSpecified()
    {
        // Arrange
        var configuration = new SecretVariableProviderConfiguration(
            SecretVariableProviderAlgorithm.RSA,
            EncryptionPadding.OaepSHA256,
            "PublicKey",
            null,
            "PrivateKey",
            "PrivateKeyPath",
            null);

        // Act
        var exception = Assert.Throws<ValidationException>(() => SecretVariableProviderDefinition.From(configuration));

        // Assert
        exception.Errors.Should().HaveCount(1);
        exception.Errors.Should().Contain("PrivateKey and PrivateKeyPath cannot be specified at the same time.");
    }
}