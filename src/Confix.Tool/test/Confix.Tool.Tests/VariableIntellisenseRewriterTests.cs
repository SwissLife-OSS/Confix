using System.Text.Json.Nodes;
using Confix.Tool.Schema;
using Json.More;
using Snapshooter.Xunit;

namespace ConfiX.Entities.Component.Configuration;

public class VariableIntellisenseRewriterTests
{
    [Fact]
    public void Rewrite_JsonObjectOfTypeString_CorrectlyRewrites()
      => ExpectRewriteToMatchSnapshot("""
      {
        "type": "string",
        "minLength": 2,
        "maxLength": 3
      }
      """);

    [Fact]
    public void Rewrite_JsonObjectBasic_CorrectlyRewrites()
      => ExpectRewriteToMatchSnapshot("""
      {
        "type": "object",
        "properties": {
          "street_address": { "type": "string" },
          "city": { "type": "string" },
          "state": { "type": "string" }
        },
        "required": ["street_address", "city", "state"]
      }
      """);

    [Fact]
    public void Rewrite_JsonObjectWithRefs_CorrectlyRewrites()
      => ExpectRewriteToMatchSnapshot("""
      {
        "$id": "https://example.com/schemas/customer",

        "type": "object",
        "properties": {
          "first_name": { "$ref": "#/$defs/name" },
          "last_name": { "$ref": "#/$defs/name" },
          "shipping_address": { "$ref": "/schemas/address" },
          "billing_address": { "$ref": "/schemas/address" }
        },
        "required": ["first_name", "last_name", "shipping_address", "billing_address"],

        "$defs": {
          "name": { "type": "string" }
        }
      }
      """);

    private static void ExpectRewriteToMatchSnapshot(string inputSchema)
    {
        // arrange
        var obj = JsonNode.Parse(inputSchema)!;
        var context = new VariableIntellisenseContext(
          new JsonObject
          {
              ["$ref"] = "#/definitions/variable"
          }
        );
        var sut = new VariableIntellisenseRewriter();

        // act
        var result = sut.Rewrite(obj, context);

        // assert
        Snapshot.Match(result.ToJsonString(new() { WriteIndented = true }));
    }
}