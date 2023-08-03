using System.Text.Json.Nodes;
using Confix.Inputs;
using Confix.Variables;
using FluentAssertions;
using Json.More;

namespace Confix.Tool.Tests;

public class LocalVariableProviderTests : IDisposable
{
    private readonly string tmpFilePath = $"./LocalVariableProviderTests_{Guid.NewGuid():N}.json";

    [Fact]
    public async Task ListAsync_ValidFile_CorrectResult()
    {
        // arrange
        await PrepareFile(
            """
            {
                "foo": 42,
                "bar": "baz"
            }
            """);
        LocalVariableProvider provider = new(new LocalVariableProviderDefinition(tmpFilePath));

        // act
        var result = await provider.ListAsync(default);

        // assert
        result.Should().HaveCount(2);
        result.Should().Contain("foo");
        result.Should().Contain("bar");
    }

    [Fact]
    public async Task ResolveAsync_ExistingPath_CorrectResult()
    {
        // arrange
        await PrepareFile(
            """
            {
                "foo": 42,
                "bar": "baz"
            }
            """);
        LocalVariableProvider provider = new(new LocalVariableProviderDefinition(tmpFilePath));

        // act
        var result = await provider.ResolveAsync("foo", default);

        // assert
        Assert.True(result.IsEquivalentTo(JsonValue.Create(42)));
    }

    [Fact]
    public async Task ResolveAsync_WithArray_CorrectResult()
    {
        // arrange
        await PrepareFile(
            """
            {
                "someArray": ["a", "b", "c"]
            }
            """);
        LocalVariableProvider provider = new(new LocalVariableProviderDefinition(tmpFilePath));

        // act
        var result = await provider.ResolveAsync("someArray", default);

        // assert
        Assert.True(result.IsEquivalentTo(JsonNode.Parse("""["a","b","c"]""")));
    }

    [Fact]
    public async Task ResolveAsync_NonExistingPath_VariableNotFoundException()
    {
        // arrange
        await PrepareFile(
            """
            {
                "foo": 42,
                "bar": "baz"
            }
            """);
        LocalVariableProvider provider = new(new LocalVariableProviderDefinition(tmpFilePath));

        // act & assert
        await Assert.ThrowsAsync<VariableNotFoundException>(()
            => provider.ResolveAsync("nonexistent", default));
    }

    [Fact]
    public async Task ResolveManyAsync_ExistingPaths_CorrectResult()
    {
        // arrange
        await PrepareFile(
            """
            {
                "foo": 42,
                "bar": "baz"
            }
            """);
        LocalVariableProvider provider = new(new LocalVariableProviderDefinition(tmpFilePath));
        var paths = new List<string> { "foo", "bar" };

        // act
        var result = await provider.ResolveManyAsync(paths, default);

        // assert
        result.Should().HaveCount(2);
        Assert.True(result["foo"].IsEquivalentTo(JsonValue.Create(42)));
        Assert.True(result["bar"].IsEquivalentTo(JsonValue.Create("baz")));
    }

    [Fact]
    public async Task ResolveManyAsync_MixedPaths_AggregateException()
    {
        // arrange
        await PrepareFile(
            """
            {
                "foo": 42,
                "bar": "baz"
            }
            """);
        LocalVariableProvider provider = new(new LocalVariableProviderDefinition(tmpFilePath));
        var paths = new List<string> { "foo", "nonexistent" };

        // act & assert
        var exception =
            await Assert.ThrowsAsync<AggregateException>(()
                => provider.ResolveManyAsync(paths, default));
        exception.InnerExceptions.Should().HaveCount(1);
        exception.InnerExceptions[0].Should().BeOfType<VariableNotFoundException>();
    }

    [Fact]
    public async Task ListAsync_Should_NotFail_When_NoFile()
    {
        // arrange
        LocalVariableProvider provider = new(new LocalVariableProviderDefinition(tmpFilePath));

        // act
        var result = await provider.ListAsync(default);

        // assert
        result.Should().HaveCount(0);
    }

    [Fact]
    public async Task ListAsync_Should_NotCreateFile_When_FileDoesNotExists()
    {
        // arrange
        LocalVariableProvider provider = new(new LocalVariableProviderDefinition(tmpFilePath));

        // act
        await provider.ListAsync(default);

        // assert
        Assert.False(File.Exists(tmpFilePath));
    }

    [Fact]
    public async Task GetAsync_Should_CreateFile_When_FileDoesNotExist()
    {
        // arrange
        LocalVariableProvider provider = new(new LocalVariableProviderDefinition(tmpFilePath));

        // act
        var exception =
            await Record.ExceptionAsync(async () => await provider.ResolveAsync("foo", default));

        // assert
        Assert.IsType<VariableNotFoundException>(exception);
        Assert.True(File.Exists(tmpFilePath));
    }

    [Fact]
    public async Task SetAsync_Should_SetAndReturn()
    {
        // arrange
        LocalVariableProvider provider = new(new LocalVariableProviderDefinition(tmpFilePath));

        // act
        var result = await provider.SetAsync("foo", JsonValue.Create("bar")!, default);

        // assert
        result.Should().Be("foo");
        Assert.Equal(
            """
            {
              "foo": "bar"
            }
            """,
            await File.ReadAllTextAsync(tmpFilePath));
    }

    [Fact]
    public async Task SetAsync_Should_SetAndReturn_Deep()
    {
        // arrange
        LocalVariableProvider provider = new(new LocalVariableProviderDefinition(tmpFilePath));

        // act
        var result = await provider.SetAsync("foo.bar.baz", JsonValue.Create("bar")!, default);

        // assert
        result.Should().Be("foo.bar.baz");
        Assert.Equal(
            """
            {
              "foo": {
                "bar": {
                  "baz": "bar"
                }
              }
            }
            """,
            await File.ReadAllTextAsync(tmpFilePath));
    }

    [Fact]
    public async Task SetAsync_Should_SetAndReturn_When_FileHasContent()
    {
        // arrange
        await PrepareFile(
            """
            {
              "foo": "bar"
            }
            """);
        LocalVariableProvider provider = new(new LocalVariableProviderDefinition(tmpFilePath));

        // act
        var result = await provider.SetAsync("foo", JsonValue.Create("baz")!, default);

        // assert
        result.Should().Be("foo");
        Assert.Equal(
            """
            {
              "foo": "baz"
            }
            """,
            await File.ReadAllTextAsync(tmpFilePath));
    }

    [Fact]
    public async Task SetAsync_Should_SetAndReturn_When_FileHasContentDeep()
    {
        // arrange
        await PrepareFile(
            """
            {
              "foo": {
                "bar": {
                  "baz": "bar"
                }
              }
            }
            """);

        LocalVariableProvider provider = new(new LocalVariableProviderDefinition(tmpFilePath));

        // act
        var result = await provider.SetAsync("foo.bar.baz", JsonValue.Create("baz")!, default);

        // assert
        result.Should().Be("foo.bar.baz");
        Assert.Equal(
            """
            {
              "foo": {
                "bar": {
                  "baz": "baz"
                }
              }
            }
            """,
            await File.ReadAllTextAsync(tmpFilePath));
    }

    [Fact]
    public async Task SetAsync_Should_ThrowExitException_When_FileHasArray()
    {
        // arrange
        await PrepareFile(
            """
            {
              "foo": [
                { "bar": "baz"}
              ]
            }
            """);

        LocalVariableProvider provider = new(new LocalVariableProviderDefinition(tmpFilePath));

        // act & assert
        await Assert.ThrowsAsync<ExitException>(()
            => provider.SetAsync("foo.bar", JsonValue.Create("baz")!, default));
    }

    private Task PrepareFile(string content) => File.WriteAllTextAsync(tmpFilePath, content);

    public void Dispose()
    {
        File.Delete(tmpFilePath);
    }
}
