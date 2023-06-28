using System.Text.Json.Nodes;
using Confix.Variables;
using FluentAssertions;
using Snapshooter.Xunit;

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