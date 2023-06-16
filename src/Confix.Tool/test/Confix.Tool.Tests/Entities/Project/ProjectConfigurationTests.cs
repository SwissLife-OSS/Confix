using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Middlewares;
using Confix.Tool.Schema;
using Snapshooter.Xunit;

namespace ConfiX.Entities.Component.Configuration;

public class ProjectConfigurationTests : ParserTestBase
{
    [Fact]
    public void Parse_Should_BeValid()
    {
        ExpectValid(
            """
            {
                "name": "TestProject",
                "environments": [
                    {
                        "name": "development",
                        "excludeFiles": ["appsettings.staging.json"]
                    },
                    "staging",
                    {
                        "name": "production",
                        "excludeFiles": ["appsettings.staging.json"]
                    }
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
        """);
    }

    [Fact]
    public void Parse_Should_ParseSubProjects_When_Provided()
    {
        ExpectValid(
            """
            {
                "name": "TestProject",
                "subProjects": [
                    {
                        "name": "SubProject1"
                    },
                    {
                        "name": "SubProject2"
                    }
                ]
            }
            """);
    }

    [Fact]
    public void Parse_Should_BeValid_When_NonRequiredArMissing()
    {
        ExpectValid("{}");
    }

    [Fact]
    public void Parse_Should_BeValid_When_ComponentLatest()
    {
        ExpectValid(
            """
            {
                "name": "TestProject",
                "components": {
                    "@dotnet-package/BlobStorage": "latest"
                }
            }
            """);
    }

    [Fact]
    public void Parse_Should_BeValid_When_ComponentVersion()
    {
        ExpectValid(
            """
            {
                "name": "TestProject",
                "components": {
                    "@dotnet-package/BlobStorage": "1.0.0"
                }
            }
            """);
    }

    [Fact]
    public void Parse_Should_BeValid_When_ComponentEnabled()
    {
        ExpectValid(
            """
            {
                "name": "TestProject",
                "components": {
                    "@dotnet-package/BlobStorage": true
                }
            }
            """);
    }

    [Fact]
    public void Parse_Should_BeValid_When_ComponentDisabled()
    {
        ExpectValid(
            """
            {
                "name": "TestProject",
                "components": {
                    "@dotnet-package/BlobStorage": false
                }
            }
            """);
    }

    [Fact]
    public void Merge_Should_ReturnOriginalConfiguration_When_OtherConfigurationIsNull()
    {
        // Arrange
        var original = new ProjectConfiguration(
            "TestProject",
            new List<EnvironmentConfiguration>(),
            new List<ComponentReferenceConfiguration>(),
            new List<ComponentRepositoryConfiguration>(),
            new List<VariableProviderConfiguration>(),
            new List<ComponentProviderConfiguration>(),
            new List<ConfigurationFileConfiguration>(),
            new List<ProjectConfiguration>(),
            Array.Empty<JsonFile>()
        );

        // Act
        var merged = original.Merge(null);

        // Assert
        Assert.Same(original, merged);
    }

    [Fact]
    public void Merge_Should_ReturnMergedConfiguration_When_OtherConfigurationIsNotNull()
    {
        // Arrange
        var original = new ProjectConfiguration(
            "TestProject",
            new List<EnvironmentConfiguration>
            {
                new("env1", new List<string> { "file1" }, new List<string> { "file2" }, false),
            },
            new List<ComponentReferenceConfiguration>
            {
                new("test", "test", "test", true, null),
            },
            new List<ComponentRepositoryConfiguration>
            {
                new("repo", "type", JsonNode.Parse("{}")!.AsObject()),
            },
            new List<VariableProviderConfiguration>
            {
                new("Provider2", "Type2", null, new JsonObject())
            },
            new List<ComponentProviderConfiguration>
            {
                new("Provider1", "Type1", new JsonObject()),
            },
            new List<ConfigurationFileConfiguration>
            {
                new("type", JsonNode.Parse("{}")!.AsObject()),
            },
            new List<ProjectConfiguration>
            {
                new("Subproject1",
                    new List<EnvironmentConfiguration>
                    {
                        new("env1", new List<string> { "file1" }, new List<string> { "file2" }, false),
                    },
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    Array.Empty<JsonFile>())
            },
            Array.Empty<JsonFile>());
        var other = new ProjectConfiguration(
            "MergedProject",
            new List<EnvironmentConfiguration>
            {
                new("env1", new List<string> { "file2" }, new List<string> { "file3" }, true),
            },
            new List<ComponentReferenceConfiguration>
            {
                new("test", "test", "test", false, null),
            },
            new List<ComponentRepositoryConfiguration>
            {
                new("repo", "type", JsonNode.Parse(""" {"foo": "bar"} """)!.AsObject()),
            },
            new List<VariableProviderConfiguration>
            {
                new("Provider2", "Type2", null, JsonNode.Parse(""" {"foo": "bar"} """)!.AsObject())
            },
            new List<ComponentProviderConfiguration>
            {
                new("Provider1", "Type1", JsonNode.Parse(""" {"foo": "bar"} """)!.AsObject()),
            },
            new List<ConfigurationFileConfiguration>
            {
                new("type", JsonNode.Parse(""" {"foo": "bar"} """)!.AsObject()),
            },
            new List<ProjectConfiguration>
            {
                new("Subproject1",
                    new List<EnvironmentConfiguration>
                    {
                        new("env1", new List<string> { "file2" }, new List<string> { "file3" }, false),
                    },
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    Array.Empty<JsonFile>()),
            },
            Array.Empty<JsonFile>()
        );

        // Act
        var merged = original.Merge(other);

        // Assert
        Assert.NotSame(original, merged);
        Assert.Equal("MergedProject", merged.Name);
        Assert.Single(merged.Environments!);
        Assert.Equal("file2", merged.Environments!.First().ExcludeFiles!.First());
        Assert.Single(merged.Components!);
        Assert.False(merged.Components!.First().IsEnabled);
        Assert.Single(merged.Repositories!);
        Assert.Equal("bar", merged.Repositories!.First().Values!["foo"]!.ToString());

        Assert.Single(merged.VariableProviders!);
        Assert.Equal("bar", merged.VariableProviders!.First().Values!["foo"]!.ToString());

        Assert.Single(merged.ComponentProviders!);
        Assert.Equal("bar", merged.ComponentProviders!.First().Values!["foo"]!.ToString());

        Assert.Single(merged.Subprojects!);
        Assert.Equal("Subproject1", merged.Subprojects!.First().Name);
        Assert.Single(merged.Subprojects!.First().Environments!);
        Assert.Equal("file2",
            merged.Subprojects!.First().Environments!.First().ExcludeFiles!.First());
    }

    [Fact]
    public void LoadFromFiles_Should_ReturnNull_When_ConfigurationFileNotPresent()
    {
        // Arrange
        var files = Array.Empty<JsonFile>();

        // Act
        var result = ProjectConfiguration.LoadFromFiles(files);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoadFromFiles_Should_LoadConfigurationFromFile()
    {
        // Arrange
        var confixRcPath = Path.Combine(
            Path.GetTempPath(),
            Path.GetRandomFileName(),
            FileNames.ConfixProject);
        Directory.CreateDirectory(Path.GetDirectoryName(confixRcPath)!);

        File.WriteAllText(confixRcPath,
            """
                {
                    "name": "TestProject",
                    "environments": [
                        {
                            "name": "development",
                            "excludeFiles": ["appsettings.staging.json"]
                        },
                        "staging",
                        {
                            "name": "production",
                            "excludeFiles": ["appsettings.staging.json"]
                        }
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
            """);

        var files = new List<JsonFile>
        {
           await JsonFile.FromFile(new(confixRcPath), default)
        };
        // Act
        var result = ProjectConfiguration.LoadFromFiles(files);

        // Assert
        Assert.NotNull(result);

        new
        {
            result.Components,
            result.ComponentProviders,
            result.Repositories,
            result.ConfigurationFiles,
            result.Environments,
            result.VariableProviders,
            result.Name,
            result.Subprojects,
            SourceFiles = result.SourceFiles.Select(x => x.File.FullName)
        }.MatchSnapshot(o => o.IgnoreField("SourceFiles"));

        // Cleanup
        File.Delete(confixRcPath);
    }

    public override object Parse(JsonNode json)
    {
        return ProjectConfiguration.Parse(json);
    }
}
