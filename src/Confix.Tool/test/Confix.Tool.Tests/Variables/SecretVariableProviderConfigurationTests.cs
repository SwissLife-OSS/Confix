using System.Text.Json.Nodes;
using ConfiX.Variables;
using FluentAssertions;

namespace Confix.Tool.Tests;

public class SecretVariableProviderConfigurationTests
{

    [Fact]
    public void Parse_EmptyObject_DefaultAlgorithm()
    {
        // arrange
        var configuration = JsonNode.Parse("{}")!;

        // act
        var result = SecretVariableProviderConfiguration.Parse(configuration);

        // assert
        result.Should().Be(new SecretVariableProviderConfiguration
        {
            Algorithm = SecretVariableProviderAlgorithm.RSA,
        });
    }

    [Fact]
    public void Parse_WithValidConfigurationKey_ReturnsValidObject()
    {
        // arrange
        var configuration = JsonNode.Parse(
            """
            {
                "algorithm": "RSA",
                "publicKey": "SomePublicKey",
                "privateKey": "SomePrivateKey"
            }
            """
        )!;

        // act
        var result = SecretVariableProviderConfiguration.Parse(configuration);

        // assert
        result.Should().Be(new SecretVariableProviderConfiguration
        {
            Algorithm = SecretVariableProviderAlgorithm.RSA,
            PublicKey = "SomePublicKey",
            PrivateKey = "SomePrivateKey",
        });
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
        result.Should().Be(new SecretVariableProviderConfiguration
        {
            Algorithm = SecretVariableProviderAlgorithm.RSA,
            PublicKeyPath = "./pub.pem",
            PrivateKeyPath = "./priv.pem",
        });
    }
}
