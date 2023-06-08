using System.Text.Json.Nodes;
using ConfiX.Variables;
using Snapshooter.Xunit;

namespace Confix.Tool.Tests;

public class JsonVariableRewriterTests
{
    [Fact]
    public void Rewrite_ShouldReplaceAllVars()
    {
        // arrange
        JsonNode? node = JsonNode.Parse("""
            {
                "foo": {
                    "bar": "baz",
                    "test": "$test:variable.number"
                },
                "bar": ["$test:variable.string"],
                "number": 42,
                "bool": "$test:variable.bool"
            }
        """);
        var variableLookup = new Dictionary<VariablePath, JsonValue> {
            { VariablePath.Parse("$test:variable.string"), JsonValue.Create("someReplacedValue")},
            { VariablePath.Parse("$test:variable.number"), JsonValue.Create(420)},
            { VariablePath.Parse("$test:variable.bool"), JsonValue.Create(true)},
        };

        // act
        var result = new JsonVariableRewriter(variableLookup).Rewrite(node);

        // assert
        result?.ToString().MatchSnapshot();
    }
}
