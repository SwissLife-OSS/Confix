using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using Confix.Entities.Component.Configuration;
using Confix.Extensions;
using Confix.Inputs;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Snapshooter.Xunit;

namespace ConfiX.Commands.Config;

public class ConfigShowCommandTests
{
    private readonly TestConfixCommandline _cli;

    public ConfigShowCommandTests()
    {
        _cli = new TestConfixCommandline();
    }

    [Fact]
    public async Task Should_PrintConfig()
    {
        // Arrange
        using var cli = _cli;

        cli.Directories.Home.CreateConfixRc(_confixRc);

        // Act
        await cli.RunAsync("config show");

        // Assert
        SnapshotBuilder
            .New()
            .Append("parsed", cli.Console.Output)
            .AddReplacement(_cli.Directories.Home.FullName, "HOME")
            .MatchSnapshot();
    }

    [Fact]
    public async Task Should_Output_Should_Be_Parsable()
    {
        // Arrange
        using var cli = _cli;

        cli.Directories.Home.CreateConfixRc(_confixRc);

        // Act
        await cli.RunAsync("config show");

        // Assert
        var output = cli.Console.Output;
        var outputConfig = RuntimeConfiguration.Parse(JsonNode.Parse(output));
        SnapshotBuilder
            .New()
            .Append("parsed", outputConfig.ToJsonString())
            .AddReplacement(_cli.Directories.Home.FullName, "HOME")
            .MatchSnapshot();
    }

    [StringSyntax("json")]
    private const string _confixRc = """
        {
          "component": {
            "name": "__Default",
            "inputs": [
              {
                "type": "dotnet",
                "additional": "property"
              },
              {
                "type": "graphql",
                "additional": "property"
              }
            ]
          },
          "isRoot": false,
          "encryption": {
            "provider": {
              "type": "TestProvider",
              "additional": "property"
            }
          },
          "project": {
            "name": "TestProject",
            "environments": [
              "development",
              "staging",
              "production"
            ],
            "components": {
              "@dotnet-package/BlobStorage": {
                "mountingPoint": [
                  "documents/blob-storage",
                  "user-data/blob-storage"
                ]
              },
              "@oss-components/CustomComponent": "1.0.0"
            },
            "componentProviders": [
              {
                "name": "dotnet",
                "type": "dotnet-package",
                "additional": "property"
              }
            ],
            "componentRepositories": [
              {
                "name": "common-components",
                "type": "git",
                "additional": "property"
              }
            ],
            "configurationFiles": [
              "./**/some-config/appsettings*.json",
              {
                "type": "dotnet-appsettings",
                "additional": "property"
              }
            ],
            "variableProviders": [
              {
                "name": "appsettings",
                "type": "dotnet-appsettings",
                "environmentOverride": {
                  "dev": {
                    "file": "appsettings.dev.json"
                  },
                  "prod": {
                    "file": "appsettings.prod.json"
                  }
                }
              }
            ]
          }
        }
        """;
}
