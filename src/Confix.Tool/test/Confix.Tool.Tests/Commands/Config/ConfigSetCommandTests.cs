using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using Confix.Inputs;
using FluentAssertions;

namespace ConfiX.Commands.Config;

public class ConfigSetCommandTests : IAsyncLifetime
{
    private readonly TestConfixCommandline _cli = new();

    [Fact]
    public async Task Should_Set_Scalar()
    {
        // Arrange
        var confixRc = _cli.Directories.Home.CreateConfixRc(_confixRc);

        // Act
        await _cli.RunAsync("config set encryption.provider.type \"Foo\"");

        // Assert
        var content = await File.ReadAllTextAsync(confixRc.FullName);
        var parsed = JsonNode.Parse(content)!;
        parsed["encryption"]!["provider"]!["type"]?.GetValue<string>().Should().Be("Foo");
    }

    [Fact]
    public async Task Should_Set_Object()
    {
        // Arrange
        var confixRc = _cli.Directories.Home.CreateConfixRc(_confixRc);

        // Act
        await _cli.RunAsync("config",
            "set",
            "encryption",
            """ { "provider": { "type": "Foo" } } """);

        // Assert
        var content = await File.ReadAllTextAsync(confixRc.FullName);
        var parsed = JsonNode.Parse(content)!;
        parsed["encryption"]!["provider"]!["type"]?.GetValue<string>().Should().Be("Foo");
    }

    [Fact]
    public async Task Should_Set_In_ClosestConfixRc()
    {
        // Arrange
        var confixRc = _cli.Directories.Home.CreateConfixRc(_confixRc);
        var closestConfixRc = _cli.Directories.Content.CreateConfixRc(_confixRc);

        // Act
        await _cli.RunAsync("config set encryption.provider \"Foo\"");

        // Assert
        var content = await File.ReadAllTextAsync(closestConfixRc.FullName);
        var parsed = JsonNode.Parse(content)!;
        parsed["encryption"]!["provider"]?.GetValue<string>().Should().Be("Foo");
        var homeContent = await File.ReadAllTextAsync(confixRc.FullName);
        Assert.Equal(_confixRc, homeContent);
    }

    [StringSyntax("json")]
    private const string _confixRc = """
        {
          "encryption": {
            "provider": {
              "type": "TestProvider",
              "additional": "property"
            }
          }
        }
        """;

    /// <inheritdoc />
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DisposeAsync()
    {
        _cli.Dispose();
        return Task.CompletedTask;
    }
}
