using System.Text.Json.Nodes;
using ConfiX.Extensions;
using Confix.Tool.Abstractions;
using Confix.Tool.Schema;

namespace ConfiX.Entities.Component.Configuration;

public class ConfixConfigurationTest : ParserTestBase
{
    [Fact]
    public void Parse_Should_BeValid()
    {
        ExpectValid(
            """
            {
                "project": {
                    "name": "ConfiX"
                },
                "component": {
                   "name": "TestComponent"
                },
                "isRoot": true
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
        var original = new ConfixConfiguration(true,
            ProjectConfiguration.Empty,
            null,
            Array.Empty<FileInfo>());

        // Act
        var merged = original.Merge(null);

        // Assert
        Assert.Same(original, merged);
    }

    [Fact]
    public void Merge_Should_ReturnMergedConfiguration_When_OtherConfigurationIsNotNull()
    {
        // Arrange
        var original = new ConfixConfiguration(
            true,
            ProjectConfiguration.Empty,
            new ComponentConfiguration(
                "TestComponent",
                new List<ComponentInputConfiguration>(),
                new List<ComponentOutputConfiguration>(),
                Array.Empty<FileInfo>()),
            Array.Empty<FileInfo>());
        var other = new ConfixConfiguration(
            false,
            new ProjectConfiguration("MergedProject",
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                Array.Empty<FileInfo>()),
            new ComponentConfiguration("MergedComponent",
                new List<ComponentInputConfiguration>(),
                new List<ComponentOutputConfiguration>(),
                Array.Empty<FileInfo>()),
            Array.Empty<FileInfo>());

        // Act
        var merged = original.Merge(other);

        // Assert
        Assert.NotSame(original, merged);
        Assert.True(merged.IsRoot);
        Assert.Equal("MergedProject", merged.Project?.Name);
        Assert.Equal("MergedComponent", merged.Component?.Name);
    }

    [Fact]
    public void LoadFromFiles_Should_LoadConfigurationFromFile_ShouldMerge()
    {
        // Arrange
        var confixRcPath1 =
            Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), FileNames.ConfixRc);
        var confixRcPath2 =
            Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), FileNames.ConfixRc);

        Directory.CreateDirectory(Path.GetDirectoryName(confixRcPath1)!);
        Directory.CreateDirectory(Path.GetDirectoryName(confixRcPath2)!);

        var configuration = new List<FileInfo>
        {
            new(confixRcPath1),
            new(confixRcPath2)
        };

        File.WriteAllText(confixRcPath1,
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
                }
            }
            """);
        File.WriteAllText(confixRcPath2,
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
                }
            }
            """);

        // Act
        var result = ConfixConfiguration.LoadFromFiles(configuration);

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
    public void LoadFromFiles_Should_LoadAndMergeConfigurationsFromMultipleFiles()
    {
        // Arrange
        var confixRcPath1 =
            Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), FileNames.ConfixRc);
        var confixRcPath2 =
            Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), FileNames.ConfixRc);
        var confixRcPath3 =
            Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), FileNames.ConfixRc);

        var configuration = new List<FileInfo>
        {
            new(confixRcPath1),
            new(confixRcPath2),
            new(confixRcPath3)
        };

        Directory.CreateDirectory(Path.GetDirectoryName(confixRcPath1)!);
        Directory.CreateDirectory(Path.GetDirectoryName(confixRcPath2)!);
        Directory.CreateDirectory(Path.GetDirectoryName(confixRcPath3)!);

        File.WriteAllText(confixRcPath1,
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
        File.WriteAllText(confixRcPath2,
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
        File.WriteAllText(confixRcPath3,
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

        // Act
        var result = ConfixConfiguration.LoadFromFiles(configuration);

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

    public override object Parse(JsonNode json)
    {
        return ConfixConfiguration.Parse(json);
    }
}
