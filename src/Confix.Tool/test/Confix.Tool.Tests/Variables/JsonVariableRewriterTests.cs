using System.Text.Json.Nodes;
using Confix.Variables;
using Snapshooter.Xunit;

namespace Confix.Tool.Tests;

public class JsonVariableRewriterTests
{
    [Fact]
    public void Rewrite_ShouldReplaceAllVars()
    {
        // arrange
        JsonNode node = JsonNode.Parse("""
            {
                "foo": {
                    "bar": "baz",
                    "test": "$test:variable.number"
                },
                "bar": ["$test:variable.string"],
                "number": 42,
                "stringWithMultiple": "asterix-{{$test:variable.bool}}-oberlix-midefix-{{$test:variable.string}}-confix",
                "bool": "$test:variable.bool",
                "array": "$test:variable.array",
                "arrayElement": [
                    "a",
                    {
                        "foo": "$test:variable.string"
                    },
                    "$test:variable.number"
                ]
            }
        """)!;
        var variableLookup = new Dictionary<VariablePath, JsonNode> {
            { VariablePath.Parse("$test:variable.string"), JsonValue.Create("someReplacedValue")!},
            { VariablePath.Parse("$test:variable.number"), JsonValue.Create(420)},
            { VariablePath.Parse("$test:variable.bool"), JsonValue.Create(true)},
            { VariablePath.Parse("$test:variable.array"), JsonNode.Parse("""["a", "b", "c"]""")!},
        };

        // act
        var result = new JsonVariableRewriter().Rewrite(node, new(variableLookup));

        // assert
        result?.ToString().MatchSnapshot();
    }
}
