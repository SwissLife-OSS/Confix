using System.Text.Json.Nodes;
using Confix.Extensions;
using Confix.Extensions;
using Confix.Tool.Abstractions;
using Confix.Tool.Schema;
using Confix.Tool.Middlewares;
using Confix.Tool.Reporting;

namespace Confix.Entities.Component.Configuration;

public class RuntimeConfigurationTests : ParserTestBase
{
    [Fact]
    public void Parse_Should_BeValid()
    {
        ExpectValid(
            """
            {
                "project": {
                    "name": "Confix"
                },
                "component": {
                   "name": "TestComponent"
                },
                "isRoot": true,
                "encryption": {
                    "provider": {
                        "type": "TestProvider"
                    }
                },
                "reporting": {
                    "dependencies": {
                        "providers": [{
                            "kind": "test",
                            "type": "bar",
                            "x": "x",
                            "y": "y"
                        }]
                    }
                }
            }
            """);
    }

    [Fact]
    public void Parse_Should_BeValid_When_NonRequiredArMissing()
    {
        ExpectValid("{}");
    }

    [Fact]
    public void Parse_Should_BeInvalid_When_ProjectIsNotObject()
    {
        ExpectInvalid(
            """
            {
                "project": "development"
            }
            """);
    }

    [Fact]
    public void Parse_Should_BeInvalid_When_ComponentNotObject()
    {
        ExpectInvalid(
            """
            {
                "component": "component"
            }
            """);
    }

    [Fact]
    public void Parse_Should_BeInvalid_When_IsRootIsNotBoolean()
    {
        ExpectInvalid(
            """
            {
                "isRoot": "true"
            }
            """);
    }

    [Fact]
    public void Merge_Should_ReturnOriginalConfiguration_When_OtherConfigurationIsNull()
    {
        // Arrange
        var original = new RuntimeConfiguration(
            true,
            ProjectConfiguration.Empty,
            null,
            null,
            null,
            Array.Empty<JsonFile>());

        // Act
        var merged = original.Merge(null);

        // Assert
        Assert.Same(original, merged);
    }

    [Fact]
    public void Merge_Should_ReturnMergedConfiguration_When_OtherConfigurationIsNotNull()
    {
        // Arrange
        var original = new RuntimeConfiguration(
            true,
            ProjectConfiguration.Empty,
            new ComponentConfiguration(
                "TestComponent",
                new List<ComponentInputConfiguration>(),
                new List<ComponentOutputConfiguration>(),
                Array.Empty<JsonFile>()),
            new EncryptionConfiguration(
                new EncryptionProviderConfiguration(
                    "test",
                    new Dictionary<string, JsonObject>(),
                    JsonNode.Parse("""{}""")!.AsObject())),
            new ReportingConfiguration(new ReportingDependencyConfiguration(new[]
            {
                new DependencyProviderConfiguration("test", JsonNode.Parse("{}").AsObject())
            })),
            Array.Empty<JsonFile>());
        var other = new RuntimeConfiguration(
            false,
            new ProjectConfiguration("MergedProject",
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                Array.Empty<JsonFile>()),
            new ComponentConfiguration("MergedComponent",
                new List<ComponentInputConfiguration>(),
                new List<ComponentOutputConfiguration>(),
                Array.Empty<JsonFile>()),
            null,
            null,
            Array.Empty<JsonFile>());

        // Act
        var merged = original.Merge(other);

        // Assert
        Assert.NotSame(original, merged);
        Assert.True(merged.IsRoot);
        Assert.Equal("MergedProject", merged.Project?.Name);
        Assert.Equal("MergedComponent", merged.Component?.Name);
        Assert.Equal("test", merged.Encryption?.Provider?.Type);
    }

    [Fact]
    public async Task LoadFromFiles_Should_LoadConfigurationFromFile_ShouldMergeAsync()
    {
        // Arrange
        var confixRcPath1 =
            Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), FileNames.ConfixRc);
        var confixRcPath2 =
            Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), FileNames.ConfixRc);

        Directory.CreateDirectory(Path.GetDirectoryName(confixRcPath1)!);
        Directory.CreateDirectory(Path.GetDirectoryName(confixRcPath2)!);
        await File.WriteAllTextAsync(confixRcPath1,
            """
            {
                "isRoot": false,
                "component": {
                    "name": "TestComponent",
                    "inputs": [],
                    "outputs": []
                },
                "project": {
                    "name": "TestProject"
                },
                "reporting": {
                    "dependencies": {
                        "providers": [{
                            "kind": "test",
                            "type": "bar",
                            "x": "x",
                            "y": "y"
                        }]
                    }
                }
            }
            """);
        await File.WriteAllTextAsync(confixRcPath2,
            """
            {
                "isRoot": false,
                "component": {
                    "name": "MergedComponent",
                    "inputs": [],
                    "outputs": []
                },
                "project": {
                    "name": "MergedProject"
                },
                "reporting": {
                    "dependencies": {
                        "providers": [{
                            "kind": "test",
                            "type": "bar",
                            "x": "m",
                            "z": "z"
                        }]
                    }
                }
            }
            """);

        var configuration = new List<JsonFile>
        {
            await JsonFile.FromFile(new(confixRcPath1), default),
            await JsonFile.FromFile(new(confixRcPath2), default)
        };

        // Act
        var result = RuntimeConfiguration.LoadFromFiles(configuration);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsRoot);
        Assert.Equal("MergedComponent", result.Component?.Name);
        Assert.Equal("MergedProject", result.Project?.Name);

        // Cleanup
        File.Delete(confixRcPath1);
        File.Delete(confixRcPath2);
    }

    [Fact]
    public async Task LoadFromFiles_Should_LoadAndMergeConfigurationsFromMultipleFilesAsync()
    {
        // Arrange
        var confixRcPath1 =
            Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), FileNames.ConfixRc);
        var confixRcPath2 =
            Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), FileNames.ConfixRc);
        var confixRcPath3 =
            Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), FileNames.ConfixRc);

        Directory.CreateDirectory(Path.GetDirectoryName(confixRcPath1)!);
        Directory.CreateDirectory(Path.GetDirectoryName(confixRcPath2)!);
        Directory.CreateDirectory(Path.GetDirectoryName(confixRcPath3)!);

        await File.WriteAllTextAsync(confixRcPath1,
            """
            {
                "isRoot": false,
                "component": {
                    "name": "Component1",
                    "inputs": [],
                    "outputs": []
                },
                "project": {
                    "name": "Project1"
                }
            }
            """);
        await File.WriteAllTextAsync(confixRcPath2,
            """
                {
                    "isRoot": true,
                    "component": {
                        "name": "Component2",
                        "inputs": [],
                        "outputs": []
                    },
                    "project": {
                        "name": "Project2"
                    }
                }
            """);
        await File.WriteAllTextAsync(confixRcPath3,
            """
                {
                    "isRoot": false,
                    "component": {
                        "name": "Component3",
                        "inputs": [],
                        "outputs": []
                    },
                    "project": null
                }
            """);

        var configuration = new List<JsonFile>
        {
            await JsonFile.FromFile(new(confixRcPath1), default),
            await JsonFile.FromFile(new(confixRcPath2), default),
            await JsonFile.FromFile(new(confixRcPath3), default)
        };

        // Act
        var result = RuntimeConfiguration.LoadFromFiles(configuration);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsRoot);
        Assert.Equal("Component3", result.Component?.Name);
        Assert.Equal("Project2", result.Project?.Name);

        // Cleanup
        File.Delete(confixRcPath1);
        File.Delete(confixRcPath2);
        File.Delete(confixRcPath3);
    }

    [Fact]
    public async Task LoadFromFiles_Should_LoadAndMergeEmptyIntermediateShouldNotClear()
    {
        // Arrange
        var confixRcPath1 =
            Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), FileNames.ConfixRc);
        var confixRcPath2 =
            Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), FileNames.ConfixRc);
        var confixRcPath3 =
            Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), FileNames.ConfixRc);

        Directory.CreateDirectory(Path.GetDirectoryName(confixRcPath1)!);
        Directory.CreateDirectory(Path.GetDirectoryName(confixRcPath2)!);
        Directory.CreateDirectory(Path.GetDirectoryName(confixRcPath3)!);

        await File.WriteAllTextAsync(confixRcPath1,
            """
            {
                "isRoot": false,
                "component": {
                    "name": "Component1",
                    "inputs": [],
                    "outputs": []
                },
                "project": {
                    "name": "Project1"
                }
            }
            """);
        await File.WriteAllTextAsync(confixRcPath2,
            """
                {
                    "isRoot": true,
                    "component": {
                        "name": "Component2",
                        "inputs": [],
                        "outputs": []
                    },
                    "project": {
                        "name": "Project2"
                    }
                }
            """);
        await File.WriteAllTextAsync(confixRcPath3, """ { } """);

        var configuration = new List<JsonFile>
        {
            await JsonFile.FromFile(new(confixRcPath1), default),
            await JsonFile.FromFile(new(confixRcPath2), default),
            await JsonFile.FromFile(new(confixRcPath3), default)
        };

        // Act
        var result = RuntimeConfiguration.LoadFromFiles(configuration);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsRoot);
        Assert.Equal("Component2", result.Component?.Name);
        Assert.Equal("Project2", result.Project?.Name);

        // Cleanup
        File.Delete(confixRcPath1);
        File.Delete(confixRcPath2);
        File.Delete(confixRcPath3);
    }

    public override object Parse(JsonNode json)
    {
        return RuntimeConfiguration.Parse(json);
    }
}
