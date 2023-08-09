using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using Confix.Entities.Component.Configuration;
using Confix.Extensions;
using Confix.Inputs;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Snapshooter.Xunit;

namespace ConfiX.Commands.Config;

public class ConfigSetCommandTests
{
    private readonly TestConfixCommandline _cli = new();

    [Fact]
    public async Task Should_Set_Scalar()
    {
        // Arrange
        using var cli = _cli;

        var confixRc = cli.Directories.Home.CreateConfixRc(_confixRc);

        // Act
        await cli.RunAsync("config set encryption.provider.type \"Foo\"");

        // Assert
        var content = await File.ReadAllTextAsync(confixRc.FullName);
        var parsed = JsonNode.Parse(content)!;
        parsed["encryption"]!["provider"]!["type"].GetValue<string>().Should().Be("Foo");
    }

    [Fact]
    public async Task Should_Set_Object()
    {
        // Arrange
        using var cli = _cli;

        var confixRc = cli.Directories.Home.CreateConfixRc(_confixRc);

        // Act
        await cli.RunAsync("config",
            "set",
            "encryption",
            """ { "provider": { "type": "Foo" } } """);

        // Assert
        var content = await File.ReadAllTextAsync(confixRc.FullName);
        var parsed = JsonNode.Parse(content)!;
        parsed["encryption"]!["provider"]!["type"].GetValue<string>().Should().Be("Foo");
    }

    [Fact]
    public async Task Should_Set_In_ClosestConfixRc()
    {
        // Arrange
        using var cli = _cli;

        var confixRc = cli.Directories.Home.CreateConfixRc(_confixRc);
        var closestConfixRc = cli.Directories.Content.CreateConfixRc(_confixRc);

        // Act
        await cli.RunAsync("config set encryption.provider \"Foo\"");

        // Assert
        var content = await File.ReadAllTextAsync(closestConfixRc.FullName);
        var parsed = JsonNode.Parse(content)!;
        parsed["encryption"]!["provider"].GetValue<string>().Should().Be("Foo");
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
}
