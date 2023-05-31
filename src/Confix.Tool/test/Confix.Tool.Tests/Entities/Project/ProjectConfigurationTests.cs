using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;

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

    public override object Parse(JsonNode json)
    {
        return ProjectConfiguration.Parse(json);
    }
}
