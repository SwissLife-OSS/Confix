using System;
using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Abstractions.Configuration;
using Confix.Tool.Middlewares;
using Confix.Tool.Schema;

namespace ConfiX.Entities.Component.Configuration;

public class SolutionConfigurationTests : ParserTestBase
{
    [Fact]
    public void Parse_Should_BeValid()
    {
        ExpectValid(
            """
            {
                "project": {
                    "environments": [
                        "development",
                        "staging"
                    ]
                },
                "component": {
                   "name": "TestComponent"
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
    public void Merge_Should_ReturnOriginalConfiguration_When_OtherConfigurationIsNull()
    {
        // Arrange
        var original = new SolutionConfiguration(
            ProjectConfiguration.Empty,
            new ComponentConfiguration("TestComponent",
                new List<ComponentInputConfiguration>(),
                new List<ComponentOutputConfiguration>(),
                Array.Empty<JsonFile>()
            ),
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
        var original = new SolutionConfiguration(
            ProjectConfiguration.Empty,
            new ComponentConfiguration("TestComponent",
                new List<ComponentInputConfiguration>(),
                new List<ComponentOutputConfiguration>(),
                Array.Empty<JsonFile>()
            ),
            Array.Empty<JsonFile>());

        var other = new SolutionConfiguration(
            new ProjectConfiguration("MergedProject",
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
                Array.Empty<JsonFile>()
            ),
            Array.Empty<JsonFile>());

        // Act
        var merged = original.Merge(other);

        // Assert
        Assert.NotSame(original, merged);
        Assert.Equal("MergedProject", merged.Project?.Name);
        Assert.Equal("MergedComponent", merged.Component?.Name);
    }

    [Fact]
    public void LoadFromFiles_Should_ReturnNull_When_ConfixRcFileNotPresent()
    {
        // Arrange
        var configuration = Array.Empty<JsonFile>();

        // Act
        var result = SolutionConfiguration.LoadFromFiles(configuration);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoadFromFiles_Should_LoadSolutionConfigurationFromFileAsync()
    {
        // Arrange
        var confixRcPath = Path
            .Combine(Path.GetTempPath(), Path.GetRandomFileName(), FileNames.ConfixSolution);
        
        new FileInfo(confixRcPath).Directory!.Create();
        
        await File.WriteAllTextAsync(confixRcPath,
            """
                {
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

        var configuration = new List<JsonFile>
        {
            await JsonFile.FromFile(new(confixRcPath), default)
        };

        // Act
        var result = SolutionConfiguration.LoadFromFiles(configuration);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestComponent", result.Component?.Name);
        Assert.Equal("TestProject", result.Project?.Name);

        // Cleanup
        File.Delete(confixRcPath);
    }

    public override object Parse(JsonNode json)
    {
        return SolutionConfiguration.Parse(json);
    }
}
