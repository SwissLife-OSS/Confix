using System.Diagnostics.CodeAnalysis;
using Confix.Inputs;
using Confix.Tool.Schema;

namespace ConfiX.Commands.Config;

public class ConfigListCommandTests
{
    private readonly TestConfixCommandline _cli = new();

    [Fact]
    public async Task Should_ListFiles_Project()
    {
        // Arrange
        using var cli = _cli;

        cli.Directories.Home.CreateConfixRc(_confixRc);
        cli.Directories.Content.CreateConfixProject();

        // Act
        await cli.RunAsync("config list");

        // Assert
        SnapshotBuilder
            .New()
            .AddOutput(_cli)
            .AddReplacement(_cli.Directories.Home.FullName, "HOME")
            .AddReplacement(_cli.Directories.Content.FullName, "CONTENT")
            .MatchSnapshot();
    }

    [Fact]
    public async Task Should_ListFiles_Component()
    {
        // Arrange
        using var cli = _cli;

        cli.Directories.Home.CreateConfixRc(_confixRc);
        cli.Directories.Home.CreateFileInPath(
            Path.Combine(cli.Directories.Content.FullName, FileNames.ConfixComponent),
            """ { "name": "foo" } """);

        // Act
        await cli.RunAsync("config list");

        // Assert
        SnapshotBuilder
            .New()
            .AddOutput(_cli)
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
