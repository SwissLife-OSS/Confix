using System.Text.Json.Nodes;
using Confix.Utilities.Parsing;
using Snapshooter.Xunit;

namespace ConfiX.Entities.Component.Configuration;

public abstract class ParserTestBase
{
    public abstract object Parse(JsonNode json);

    public void ExpectValid(string json)
    {
        // arrange
        var node = JsonNode.Parse(json)!;

        // act
        var result = Parse(node);

        // assert
        result.ToJsonString().MatchSnapshot();
    }

    public void ExpectInvalid(string json)
    {
        // arrange
        var node = JsonNode.Parse(json)!;

        // act
        var result = Record.Exception(() => Parse(node));

        // assert
        Assert.NotNull(result);
        Assert.IsType<JsonParseException>(result);

        new
        {
            result.Message,
            Path = string.Join('/', node.GetPath())
        }.MatchSnapshot();
    }
}
