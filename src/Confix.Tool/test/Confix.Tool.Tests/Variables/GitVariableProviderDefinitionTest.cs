using ConfiX.Variables;
using FluentAssertions;

namespace Confix.Tool.Tests;

public class GitVariableProviderDefinitionTest
{
    [Fact]
    public void From_With_ValidConfiguration_Returns_CorrectResult()
    {
        // arrange
        var configuration = new GitVariableProviderConfiguration(
            "foo-bar",
            "some.file",
            new[] { "--some-arg" }
        );

        // act
        var result = GitVariableProviderDefinition.From(configuration);

        // assert
        result.Should().BeEquivalentTo(
            new GitVariableProviderDefinition(
                "foo-bar",
                "some.file",
                new[] { "--some-arg" }
            )
        );
    }

    [Fact]
    public void From_With_MissingRepositoryUrl_Throws()
    {
        // arrange
        var configuration = new GitVariableProviderConfiguration(
            null,
            "some.file",
            new[] { "--some-arg" }
        );

        // act
        var exception = Assert.Throws<ValidationException>(
            () => GitVariableProviderDefinition.From(configuration));

        // assert
        exception.Errors.Should().Contain("RepositoryUrl is required");
    }

    [Fact]
    public void From_With_NullArguments_Returns_CorrectResult()
    {
        // arrange
        var configuration = new GitVariableProviderConfiguration(
            "foo-bar",
            "some.file",
            null
        );

        // act
        var result = GitVariableProviderDefinition.From(configuration);

        // assert
        result.Should().BeEquivalentTo(
            new GitVariableProviderDefinition(
                "foo-bar",
                "some.file",
                Array.Empty<string>()
            )
        );
    }

    [Fact]
    public void From_With_MissingFilePath_Throws()
    {
        // arrange
        var configuration = new GitVariableProviderConfiguration(
            "foo-bar",
            null,
            new[] { "--some-arg" }
        );

        // act
        var exception = Assert.Throws<ValidationException>(
            () => GitVariableProviderDefinition.From(configuration));

        // assert
        exception.Errors.Should().Contain("FilePath is required");
    }

    [Fact]
    public void From_With_AllNull_Throws()
    {
        // arrange
        var configuration = new GitVariableProviderConfiguration(
            null,
            null,
            null
        );

        // act
        var exception = Assert.Throws<ValidationException>(
            () => GitVariableProviderDefinition.From(configuration));

        // assert
        exception.Errors.Should().Contain("RepositoryUrl is required");
        exception.Errors.Should().Contain("FilePath is required");
    }
}