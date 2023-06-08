using System.Text.Json.Nodes;
using ConfiX.Variables;
using Snapshooter.Xunit;

namespace Confix.Tool.Tests;

public class JsonVariableRewriterTests
{
    [Fact]
    public void Rewrite()
    {
        // arrange
        JsonNode? node = JsonNode.Parse("""
            {
                "foo": {
                    "bar": "baz",
                    "test": "$test:variable"
                },
                "bar": ["$test:variable"],
                "number": 42
            }
        """);
        var variableLookup = new Dictionary<VariablePath, JsonNode> {
            { VariablePath.Parse("$test:variable"), "someReplacedValue"}
        };

        // act
        var result = new JsonVariableRewriter(variableLookup).Rewrite(node);

        // assert
        result?.ToJsonString().MatchSnapshot();
    }
}
