using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Utilities.Parsing;

namespace ConfiX.Entities.Component.Configuration;

public class ComponentReferenceConfigurationTests
{
    [Fact]
    public void Parse_Should_ParseWithMountingPoint()
    {
        var json = """
         {
             "@charts/MyHelmComponent": "latest",
             "@charts/someOtherComponent": "latest",
             "@common-components/CloudComponent": false,
             "@dotnet-package/BlobStorage": {
                 "mountingPoint": [
                     "documents/blob-storage",
                     "user-data/blob-storage"
                 ]
             },
             "@oss-components/CustomComponent": "1.0.0"
         }
        """;

        var node = JsonNode.Parse(json);
        var key = "@dotnet-package/BlobStorage";
        var componentReferenceConfiguration =
            ComponentReferenceConfiguration.Parse(key, node![key]!);

        Assert.NotNull(componentReferenceConfiguration);
        Assert.Equal("dotnet-package", componentReferenceConfiguration.Provider);
        Assert.Equal("BlobStorage", componentReferenceConfiguration.ComponentName);
        Assert.Equal(2, componentReferenceConfiguration.MountingPoints!.Count);
        Assert.True(componentReferenceConfiguration.IsEnabled);
    }

    [Fact]
    public void Merge_Should_ReturnMergedConfiguration_When_OtherConfigurationIsNotNull()
    {
        // Arrange
        var original = new ComponentReferenceConfiguration("provider1",
            "componentName1",
            "version1",
            true,
            new List<string> { "mountingPoint1" });

        var other = new ComponentReferenceConfiguration("provider2",
            "componentName2",
            "version2",
            false,
            new List<string> { "mountingPoint2" });

        // Act
        var merged = original.Merge(other);

        // Assert
        Assert.NotNull(merged);
        Assert.Equal("provider2", merged.Provider);
        Assert.Equal("componentName2", merged.ComponentName);
        Assert.Equal("version2", merged.Version);
        Assert.False(merged.IsEnabled);
        Assert.Contains("mountingPoint2", merged.MountingPoints!);
    }

    [Fact]
    public void Parse_Should_HandleBooleanJsonNode_When_BooleanValueProvided()
    {
        var json = @"{ ""@provider/component"": true }";
        var node = JsonNode.Parse(json);
        var key = "@provider/component";

        var componentReferenceConfiguration =
            ComponentReferenceConfiguration.Parse(key, node![key]!);

        Assert.NotNull(componentReferenceConfiguration);
        Assert.Equal("provider", componentReferenceConfiguration.Provider);
        Assert.Equal("component", componentReferenceConfiguration.ComponentName);
        Assert.True(componentReferenceConfiguration.IsEnabled);
    }

    [Fact]
    public void Parse_Should_HandleStringJsonNode_When_StringValueProvided()
    {
        var json = @"{ ""@provider/component"": ""latest"" }";
        var node = JsonNode.Parse(json);
        var key = "@provider/component";

        var componentReferenceConfiguration =
            ComponentReferenceConfiguration.Parse(key, node![key]!);

        Assert.NotNull(componentReferenceConfiguration);
        Assert.Equal("provider", componentReferenceConfiguration.Provider);
        Assert.Equal("component", componentReferenceConfiguration.ComponentName);
        Assert.Equal("latest", componentReferenceConfiguration.Version);
        Assert.True(componentReferenceConfiguration.IsEnabled);
    }

    [Fact]
    public void Merge_Should_ReturnOriginalConfiguration_When_OtherConfigurationIsNull()
    {
        var original = new ComponentReferenceConfiguration("provider",
            "component",
            "version",
            true,
            new List<string> { "mountingPoint" });
        var merged = original.Merge(null!);

        Assert.Same(original, merged);
    }

    [Fact]
    public void Merge_Should_MergeMountingPoints_When_OtherConfigurationMountingPointsIsNull()
    {
        var original = new ComponentReferenceConfiguration("provider",
            "component",
            "version",
            true,
            new List<string> { "mountingPoint" });
        var other = new ComponentReferenceConfiguration("otherProvider",
            "otherComponent",
            "otherVersion",
            false,
            null);

        var merged = original.Merge(other);

        Assert.Equal("mountingPoint", merged.MountingPoints!.Single());
    }

    [Fact]
    public void Parse_Should_HandleEmptyMountingPointsArray_When_Provided()
    {
        var json = @"{ ""@provider/component"": { ""mountingPoint"": [] } }";
        var node = JsonNode.Parse(json);
        var key = "@provider/component";

        var componentReferenceConfiguration =
            ComponentReferenceConfiguration.Parse(key, node![key]!);

        Assert.NotNull(componentReferenceConfiguration);
        Assert.Equal("provider", componentReferenceConfiguration.Provider);
        Assert.Equal("component", componentReferenceConfiguration.ComponentName);
        Assert.Empty(componentReferenceConfiguration.MountingPoints!);
    }

    [Fact]
    public void Parse_Should_HandleSingleMountingPoint_When_Provided()
    {
        var json = @"{ ""@provider/component"": { ""mountingPoint"": ""singlePoint"" } }";
        var node = JsonNode.Parse(json);
        var key = "@provider/component";

        var componentReferenceConfiguration =
            ComponentReferenceConfiguration.Parse(key, node![key]!);

        Assert.NotNull(componentReferenceConfiguration);
        Assert.Equal("provider", componentReferenceConfiguration.Provider);
        Assert.Equal("component", componentReferenceConfiguration.ComponentName);
        Assert.Equal("singlePoint", componentReferenceConfiguration.MountingPoints!.Single());
    }

    [Fact]
    public void Parse_Should_ReturnEnabledComponent_When_NoIsEnabledProvided()
    {
        var json = @"{ ""@provider/component"": { ""mountingPoint"": [] } }";
        var node = JsonNode.Parse(json);

        var componentReferenceConfiguration =
            ComponentReferenceConfiguration.Parse("@provider/component", node!);

        Assert.True(componentReferenceConfiguration.IsEnabled);
    }

    [Fact]
    public void Merge_Should_KeepOriginalVersion_When_OtherConfigurationVersionIsNull()
    {
        var original = new ComponentReferenceConfiguration("provider",
            "component",
            "version",
            true,
            new List<string> { "mountingPoint" });
        var other = new ComponentReferenceConfiguration("otherProvider",
            "otherComponent",
            null,
            false,
            new List<string> { "otherMountingPoint" });

        var merged = original.Merge(other);

        Assert.Equal("version", merged.Version);
    }

    [Fact]
    public void Merge_Should_OverrideVersion_When_OtherConfigurationVersionIsNotNull()
    {
        var original = new ComponentReferenceConfiguration("provider",
            "component",
            "version",
            true,
            new List<string> { "mountingPoint" });
        var other = new ComponentReferenceConfiguration("otherProvider",
            "otherComponent",
            "otherVersion",
            false,
            new List<string> { "otherMountingPoint" });

        var merged = original.Merge(other);

        Assert.Equal("otherVersion", merged.Version);
    }

    [Fact]
    public void Parse_Should_HandleObjectNodeWithVersion_When_Provided()
    {
        var json = @"{ ""@provider/component"": { ""version"": ""1.0.0"" } }";
        var node = JsonNode.Parse(json);
        var key = "@provider/component";

        var componentReferenceConfiguration =
            ComponentReferenceConfiguration.Parse(key, node![key]!);

        Assert.NotNull(componentReferenceConfiguration);
        Assert.Equal("provider", componentReferenceConfiguration.Provider);
        Assert.Equal("component", componentReferenceConfiguration.ComponentName);
        Assert.Equal("1.0.0", componentReferenceConfiguration.Version);
    }

    [Fact]
    public void Parse_Should_HandleObjectNodeWithMultipleMountingPoints_When_Provided()
    {
        var json = @"{ ""@provider/component"": { ""mountingPoint"": [""point1"", ""point2""] } }";
        var node = JsonNode.Parse(json);
        var key = "@provider/component";

        var componentReferenceConfiguration =
            ComponentReferenceConfiguration.Parse(key, node![key]!);

        Assert.NotNull(componentReferenceConfiguration);
        Assert.Equal("provider", componentReferenceConfiguration.Provider);
        Assert.Equal("component", componentReferenceConfiguration.ComponentName);
        Assert.Equal(2, componentReferenceConfiguration.MountingPoints!.Count);
        Assert.Contains("point1", componentReferenceConfiguration.MountingPoints);
        Assert.Contains("point2", componentReferenceConfiguration.MountingPoints);
    }

    [Fact]
    public void Parse_Should_ThrowJsonParseException_When_KeyWithoutComponentName()
    {
        var json = @"{ ""@provider/"": ""value"" }";
        var node = JsonNode.Parse(json);

        Assert.Throws<JsonParseException>(()
            => ComponentReferenceConfiguration.Parse("@provider/", node!["@provider/"]!));
    }

    [Fact]
    public void Parse_Should_ThrowJsonParseException_When_KeyWithoutProvider()
    {
        var json = @"{ ""/component"": ""value"" }";
        var node = JsonNode.Parse(json);

        Assert.Throws<JsonParseException>(()
            => ComponentReferenceConfiguration.Parse("/component", node!["/component"]!));
    }
    
    [Fact]
    public void Parse_Should_ThrowJsonParseException_When_KeyWithoutProviderName()
    {
        var json = @"{ ""@/component"": ""value"" }";
        var node = JsonNode.Parse(json);

        Assert.Throws<JsonParseException>(()
            => ComponentReferenceConfiguration.Parse("/component", node!["/component"]!));
    }

    [Fact]
    public void Parse_Should_ThrowJsonParseException_When_KeyFormatIncorrect()
    {
        var json = @"{ ""provider/component"": ""value"" }";
        var node = JsonNode.Parse(json);

        Assert.Throws<JsonParseException>(()
            => ComponentReferenceConfiguration.Parse("provider/component",
                node!["provider/component"]!));
    }
}
