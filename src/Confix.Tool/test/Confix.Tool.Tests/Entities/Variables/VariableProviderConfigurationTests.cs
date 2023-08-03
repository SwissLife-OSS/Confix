using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;

namespace Confix.Entities.Component.Configuration;

public class VariableProviderConfigurationTests : ParserTestBase
{
    [Fact]
    public void Parse_Should_BeValid()
    {
        ExpectValid(
            """
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
        """);
    }

    [Fact]
    public void Parse_Should_BeValid_WhenEmpty()
    {
        ExpectValid(""" { } """);
    }

    [Fact]
    public void Parse_Should_BeInvalid_When_EnvironmentOverrideIsNotADictionary()
    {
        ExpectInvalid(
            """
            {
                "name": "appsettings",
                "type": "dotnet-appsettings",
                "environmentOverride": "dev"
            }
        """);
    }

    [Fact]
    public void Parse_Should_BeValid_When_EnvironmentOverrideIsMissing()
    {
        ExpectValid(
            """
            {
                "name": "appsettings",
                "type": "dotnet-appsettings"
            }
        """);
    }

    [Fact]
    public void Parse_Should_BeValid_When_EnvironmentOverrideIsEmpty()
    {
        ExpectValid(
            """
            {
                "name": "appsettings",
                "type": "dotnet-appsettings",
                "environmentOverride": {}
            }
        """);
    }

    [Fact]
    public void Parse_Should_BeInvalid_When_EnvironmentOverrideIsNotAnObject()
    {
        ExpectInvalid(
            """
            {
                "name": "appsettings",
                "type": "dotnet-appsettings",
                "environmentOverride": 1
            }
        """);
    }

    [Fact]
    public void Parse_Should_BeInvalid_When_EnvironmentOverrideIsNotAnObjectOfObjects()
    {
        ExpectInvalid(
            """
            {
                "name": "appsettings",
                "type": "dotnet-appsettings",
                "environmentOverride": {
                    "dev": [
                        {
                            "name": "dev",
                            "file": "appsettings.dev.json"
                        },
                        {
                            "name": "prod",
                            "file": "appsettings.prod.json"
                        }
                    ]
                }
            }
        """);
    }

    [Fact]
    public void Parse_Should_BeInvalid_When_EnvironmentOverrideContainsNonObjectValues()
    {
        ExpectInvalid(
            """
            {
                "name": "appsettings",
                "type": "dotnet-appsettings",
                "environmentOverride": {
                    "dev": "appsettings.dev.json",
                    "prod": {
                        "file": "appsettings.prod.json"
                    }
                }
            }
        """);
    }

    [Fact]
    public void Parse_Should_BeInvalid_When_EnvironmentOverrideContainsNonStringValues()
    {
        ExpectInvalid(
            """
            {
                "name": "appsettings",
                "type": "dotnet-appsettings",
                "environmentOverride": {
                    "dev": {
                        "file": "appsettings.dev.json"
                    },
                    "prod": 1
                }
            }
        """);
    }

    [Fact]
    public void Parse_Should_BeValid_When_EnvironmentOverrideIsJustAStringArray()
    {
        ExpectInvalid(
            """
            {
                "name": "appsettings",
                "type": "dotnet-appsettings",
                "environmentOverride": [
                    "dev",
                    "prod"
                ]
            }
        """);
    }

    [Fact]
    public void Parse_Should_BeInvalid_When_NameIsNotAString()
    {
        ExpectInvalid(
            """
            {
                "name": 1,
                "type": "dotnet-appsettings"
            }
        """);
    }

    [Fact]
    public void Parse_Should_BeInvalid_When_TypeIsNotAString()
    {
        ExpectInvalid(
            """
            {
                "name": "appsettings",
                "type": 1
            }
        """);
    }

    [Fact]
    public void Merge_Should_ReturnOriginalConfiguration_When_OtherConfigurationIsNull()
    {
        // Arrange
        var original = new VariableProviderConfiguration(
            "TestProvider",
            "ProviderType",
            null,
            new JsonObject());

        // Act
        var merged = original.Merge(null);

        // Assert
        Assert.Same(original, merged);
    }

    [Fact]
    public void Merge_Should_ReturnMergedConfiguration_When_OtherConfigurationIsNotNull()
    {
        // Arrange
        var original = new VariableProviderConfiguration(
            "TestProvider",
            "ProviderType",
            new Dictionary<string, JsonObject>
            {
                { "key1", new JsonObject { { "subkey1", "value1" } } },
                { "key0", new JsonObject { { "subkey2", "value2" } } }
            },
            new JsonObject());

        var other = new VariableProviderConfiguration(
            "MergedProvider",
            "MergedType",
            new Dictionary<string, JsonObject>
            {
                { "key1", new JsonObject { { "subkey2", "value2" } } },
                { "key2", new JsonObject { { "subkey2", "value2" } } }
            },
            new JsonObject());

        // Act
        var merged = original.Merge(other);

        // Assert
        Assert.NotSame(original, merged);
        Assert.Equal("MergedProvider", merged.Name);
        Assert.Equal("MergedType", merged.Type);
        Assert.Equal("value1", merged.EnvironmentOverrides?["key1"]["subkey1"]!.ToString());
        Assert.False(merged.EnvironmentOverrides?.ContainsKey("key0"));
        Assert.True(merged.EnvironmentOverrides?.ContainsKey("key1"));
        Assert.True(merged.EnvironmentOverrides?.ContainsKey("key2"));
    }

    public override object Parse(JsonNode json)
    {
        return VariableProviderConfiguration.Parse(json);
    }
}
