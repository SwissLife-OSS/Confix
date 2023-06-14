using System.Text.Json.Nodes;
using ConfiX.Variables;
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
        LocalVariableProvider provider = new(new LocalVariableProviderConfiguration() { FilePath = tmpFilePath });

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
        LocalVariableProvider provider = new(new LocalVariableProviderConfiguration() { FilePath = tmpFilePath });

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
        LocalVariableProvider provider = new(new LocalVariableProviderConfiguration() { FilePath = tmpFilePath });

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
        LocalVariableProvider provider = new(new LocalVariableProviderConfiguration() { FilePath = tmpFilePath });

        // act & assert
        await Assert.ThrowsAsync<VariableNotFoundException>(() => provider.ResolveAsync("nonexistent", default));
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
        LocalVariableProvider provider = new(new LocalVariableProviderConfiguration() { FilePath = tmpFilePath });
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
    {        // arrange
        await PrepareFile(
            """
            {
                "foo": 42,
                "bar": "baz"
            }
            """);
        LocalVariableProvider provider = new(new LocalVariableProviderConfiguration() { FilePath = tmpFilePath });
        var paths = new List<string> { "foo", "nonexistent" };

        // act & assert
        var exception = await Assert.ThrowsAsync<AggregateException>(() => provider.ResolveManyAsync(paths, default));
        exception.InnerExceptions.Should().HaveCount(1);
        exception.InnerExceptions[0].Should().BeOfType<VariableNotFoundException>();
    }

    private Task PrepareFile(string content) => File.WriteAllTextAsync(tmpFilePath, content);

    public void Dispose()
    {
        File.Delete(tmpFilePath);
    }
}
