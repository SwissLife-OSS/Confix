using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Json.More;

namespace ConfiX.Entities.Component.Configuration;

public class VariableProviderDefinitionTests
{
    [Fact]
    public void ValueWithOverrides_NoOverride_ReturnsValue()
    {
        // Arrange
        var value = new JsonObject()
        {
            ["key"] = "value"
        };
        var variableProviderDefinition = new VariableProviderDefinition(
            "name",
            "type",
            new Dictionary<string, JsonObject>(),
            value
        );

        var environmentName = "environmentName";

        // Act
        var result = variableProviderDefinition.ValueWithOverrides(environmentName);

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void ValueWithOverrides_NoInitialValue_ReturnsOverride()
    {
        // Arrange
        var envOverride = new JsonObject()
        {
            ["key"] = "value"
        };

        var variableProviderDefinition = new VariableProviderDefinition(
            "name",
            "type",
            new Dictionary<string, JsonObject>()
            {
                ["environmentName"] = envOverride
            },
            new JsonObject()
        );

        var environmentName = "environmentName";

        // Act
        var result = variableProviderDefinition.ValueWithOverrides(environmentName);

        // Assert
        Assert.True(result.IsEquivalentTo(envOverride));
    }

    [Fact]
    public void ValueWithOverrides_MergeDifferent_Valid()
    {
        // Arrange
        var envOverride = new JsonObject()
        {
            ["key"] = "value"
        };

        var value = new JsonObject()
        {
            ["key2"] = "value2"
        };

        var variableProviderDefinition = new VariableProviderDefinition(
            "name",
            "type",
            new Dictionary<string, JsonObject>()
            {
                ["environmentName"] = envOverride
            },
            value
        );

        var environmentName = "environmentName";

        // Act
        var result = variableProviderDefinition.ValueWithOverrides(environmentName);

        // Assert
        Assert.True(result.IsEquivalentTo(new JsonObject()
        {
            ["key"] = "value",
            ["key2"] = "value2"
        }));
    }

    [Fact]
    public void ValueWithOverrides_SameKey_Valid()
    {
        // Arrange
        var envOverride = new JsonObject()
        {
            ["key"] = "override"
        };

        var value = new JsonObject()
        {
            ["key"] = "base"
        };

        var variableProviderDefinition = new VariableProviderDefinition(
            "name",
            "type",
            new Dictionary<string, JsonObject>()
            {
                ["environmentName"] = envOverride
            },
            value
        );

        var environmentName = "environmentName";

        // Act
        var result = variableProviderDefinition.ValueWithOverrides(environmentName);

        // Assert
        Assert.True(result.IsEquivalentTo(new JsonObject()
        {
            ["key"] = "override",
        }));
    }
}