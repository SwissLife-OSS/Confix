using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;

namespace ConfiX.Entities.Component.Configuration;

public class ComponentProviderConfigurationTests : ParserTestBase
{
    [Fact]
    public void Parse_Should_BeValid()
    {
        ExpectValid(
            """
            {
                "name": "dotnet",
                "type": "dotnet-package",
                "additional": "property"
            }
        """);
    }

    [Fact]
    public void Parse_Should_BeValid_WhenEmpty()
    {
        ExpectValid(""" { } """);
    }

    [Fact]
    public void Merge_Should_ReturnOriginalConfiguration_When_OtherConfigurationIsNull()
    {
        // Arrange
        var original =
            new ComponentProviderConfiguration("TestProvider", "ProviderType", new JsonObject());

        // Act
        var merged = original.Merge(null);

        // Assert
        Assert.Same(original, merged);
    }

    [Fact]
    public void Merge_Should_ReturnMergedConfiguration_When_OtherConfigurationIsNotNull()
    {
        // Arrange
        var original = new ComponentProviderConfiguration(
            "TestProvider",
            "ProviderType",
            new JsonObject { { "key1", "value1" } });
        var other = new ComponentProviderConfiguration(
            "MergedProvider",
            "MergedType",
            new JsonObject { { "key2", "value2" } });

        // Act
        var merged = original.Merge(other);

        // Assert
        Assert.NotSame(original, merged);
        Assert.Equal("MergedProvider", merged.Name);
        Assert.Equal("MergedType", merged.Type);
        Assert.Equal("value1", merged.Values["key1"]!.ToString());
        Assert.Equal("value2", merged.Values["key2"]!.ToString());
    }

    /// <inheritdoc />
    public override object Parse(JsonNode json)
        => ComponentProviderConfiguration.Parse(json);
}