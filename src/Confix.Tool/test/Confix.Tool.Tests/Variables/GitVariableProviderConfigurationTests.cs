using System.Text.Json.Nodes;
using Confix.Variables;
using FluentAssertions;

namespace Confix.Tool.Tests;

public class GitVariableProviderConfigurationTests
{
    [Fact]
    public void Parse_EmptyObject_Does_NotThrow()
    {
        // arrange
        var configuration = JsonNode.Parse("{}")!;

        // act
        Action act = () => GitVariableProviderConfiguration.Parse(configuration);

        // assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Parse_With_ValidConfiguration_Returns_CorrectResult()
    {
        // arrange
        var configuration = JsonNode.Parse(
            """
            {
                "repositoryUrl": "foo-bar",
                "FilePath": "some.file",
                "arguments": ["--some-arg"]
            }
            """
        )!;

        // act
        var result = GitVariableProviderConfiguration.Parse(configuration);

        // assert
        result.Should().BeEquivalentTo(
            new GitVariableProviderConfiguration(
                "foo-bar",
                "some.file",
                new[] { "--some-arg" }
            )
        );
    }

    [Fact]
    public void Parse_InvalidKeys_Ignored()
    {
        // arrange
        var configuration = JsonNode.Parse(
            """
            {
                "repositoryUrl": "foo-bar",
                "FilePath": "some.file",
                "arguments": ["--some-arg"],
                "invalid": "key"
            }
            """
        )!;

        // act
        var result = GitVariableProviderConfiguration.Parse(configuration);

        // assert
        result.Should().BeEquivalentTo(
            new GitVariableProviderConfiguration(
                "foo-bar",
                "some.file",
                new[] { "--some-arg" }
            )
        );
    }
}
