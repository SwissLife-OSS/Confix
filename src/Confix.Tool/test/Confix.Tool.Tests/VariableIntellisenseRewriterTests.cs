using System.Text.Json.Nodes;
using Confix.Tool.Schema;
using Json.More;

namespace ConfiX.Entities.Component.Configuration;

public class VariableIntellisenseRewriterTests
{
    [Fact]
    public void Rewrite_JsonObjectOfString_CorrectlyRewrites()
    {
        // arrange
        var obj = new JsonObject
        {
            ["type"] = "string",
            ["hasVariable"] = true,
        };
        var context = new VariableIntellisenseContext(
          new JsonObject()
        );
        var sut = new VariableIntellisenseRewriter();

        // act
        var result = sut.Rewrite(obj, context);

        // assert
        Assert.True(new JsonObject
        {
            ["anyOf"] = new JsonArray{
              context.VariableReference,
              obj,
            }
        }.IsEquivalentTo(result));
    }
}