using System.Text.Json.Nodes;
using ConfiX.Variables;
using FluentAssertions;

namespace Confix.Tool.Tests;

public class GitVariableProviderConfigurationTests
{

    [Fact]
    public void Parse_EmptyObject_ThrowsArgumentException()
    {
        // arrange
        var configuration = JsonNode.Parse("{}")!;

        // act
        Action act = () => GitVariableProviderConfiguration.Parse(configuration);

        // assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Parse_WithValidConfigurationKey_ReturnsValidObject()
    {
        // arrange
        var configuration = JsonNode.Parse(
            """
            {
                "repositoryUrl": "foo-bar",
                "FilePath": "some.file"
            }
            """
        )!;

        // act
        var result = GitVariableProviderConfiguration.Parse(configuration);

        // assert
        result.Should().Be(new GitVariableProviderConfiguration
        {
            RepositoryUrl = "foo-bar",
            FilePath = "some.file"
        });
    }
}