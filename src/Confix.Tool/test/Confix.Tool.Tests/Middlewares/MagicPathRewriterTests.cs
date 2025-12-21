using System.Text.Json.Nodes;
using Confix.Inputs;
using Confix.Tool.Middlewares;
using Confix.Utilities.Json;
using Snapshooter.Xunit;

namespace Confix.Entities.Component.Configuration.Middlewares;

public class MagicPathRewriterTests
{
    private static readonly MagicPathContext _context = new(
        new DirectoryInfo("/home-location/"),
        new DirectoryInfo("/solution-location/"),
        new DirectoryInfo("/project-location/"),
        new DirectoryInfo("/file-location/")
    );

    [Fact]
    public void Rewrite_ObjectWithoutMagicStrings_Untouched()
    {
        // arrange
        var sampleObject = new JsonObject()
        {
            ["key"] = "value"
        };
        var rewriter = new MagicPathRewriter();

        // act
        var result = rewriter.Rewrite(sampleObject, _context);

        // assert
        Assert.True(sampleObject.IsEquivalentTo(result));
    }

    [Fact]
    public void Rewrite_ObjectWithMagicStrings_Replaced()
    {
        // arrange
        var sampleObject = new JsonObject()
        {
            ["fileLinux"] = "./foo/bar",
            ["fileWindows"] = """.\foo\bar""",
            ["home"] = "$home:/foo/bar",
            ["tilde"] = "~/foo/bar",
            ["solution"] = "$solution:/foo/bar",
            ["project"] = "$project:/foo/bar",
        };
        var rewriter = new MagicPathRewriter();

        // act
        var result = rewriter.Rewrite(sampleObject, _context);

        // assert
        var json = result.ToJsonString(new() { WriteIndented = true });
        Snapshot.Match(SnapshotBuilder.NormalizePaths(json));
    }

    [Fact]
    public void Rewrite_ProjectInNonProjectScope_Ignores()
    {
        // arrange
        var sampleObject = new JsonObject()
        {
            ["project"] = "$project:/foo/bar",
        };
        var rewriter = new MagicPathRewriter();

        // act
        var result = rewriter.Rewrite(sampleObject, _context with { ProjectDirectory = null });

        // assert
        Assert.True(sampleObject.IsEquivalentTo(result));
    }

    [Fact]
    public void Rewrite_SolutionInNonSolutionScope_Ignores()
    {
        // arrange
        var sampleObject = new JsonObject()
        {
            ["solution"] = "$solution:/foo/bar",
        };
        var rewriter = new MagicPathRewriter();

        // act
        var result = rewriter.Rewrite(sampleObject, _context with { SolutionDirectory = null });

        // assert
        Assert.True(sampleObject.IsEquivalentTo(result));
    }

    [Fact]
    public void Rewrite_ObjectWithNullValue_DoesNotThrow()
    {
        // arrange
        var sampleObject = new JsonObject()
        {
            ["nullValue"] = null,
            ["normalValue"] = "test"
        };
        var rewriter = new MagicPathRewriter();

        // act
        var result = rewriter.Rewrite(sampleObject, _context);

        // assert
        Assert.NotNull(result);
        var resultObj = result as JsonObject;
        Assert.NotNull(resultObj);
        Assert.True(resultObj.ContainsKey("nullValue"));
    }

    [Fact]
    public void Rewrite_ObjectWithNestedNullValues_DoesNotThrow()
    {
        // arrange
        var sampleObject = new JsonObject()
        {
            ["nested"] = new JsonObject()
            {
                ["nullValue"] = null,
                ["magicPath"] = "$home:/foo/bar"
            }
        };
        var rewriter = new MagicPathRewriter();

        // act
        var result = rewriter.Rewrite(sampleObject, _context);

        // assert
        Assert.NotNull(result);
    }

    [Fact]
    public void Rewrite_ArrayWithNullElements_DoesNotThrow()
    {
        // arrange
        var sampleObject = new JsonObject()
        {
            ["items"] = new JsonArray(
                null,
                JsonValue.Create("./foo/bar"),
                null,
                JsonValue.Create("$home:/test")
            )
        };
        var rewriter = new MagicPathRewriter();

        // act
        var result = rewriter.Rewrite(sampleObject, _context);

        // assert
        Assert.NotNull(result);
    }

    [Fact]
    public void Rewrite_MixedNullAndMagicPaths_ReplacesCorrectly()
    {
        // arrange
        var sampleObject = new JsonObject()
        {
            ["nullValue"] = null,
            ["homePath"] = "$home:/config",
            ["anotherNull"] = null,
            ["filePath"] = "./local/file"
        };
        var rewriter = new MagicPathRewriter();

        // act
        var result = rewriter.Rewrite(sampleObject, _context);

        // assert
        Assert.NotNull(result);
        var resultObj = result as JsonObject;
        Assert.NotNull(resultObj);
        
        // Null values should be preserved
        Assert.Null(resultObj["nullValue"]);
        Assert.Null(resultObj["anotherNull"]);
        
        // Magic paths should be replaced
        Assert.Contains("/home-location/", resultObj["homePath"]?.ToString());
        Assert.Contains("/file-location/", resultObj["filePath"]?.ToString());
    }

    [Fact]
    public void Rewrite_NonStringValues_Untouched()
    {
        // arrange
        var sampleObject = new JsonObject()
        {
            ["number"] = 42,
            ["boolean"] = true,
            ["decimal"] = 3.14,
            ["nullValue"] = null
        };
        var rewriter = new MagicPathRewriter();

        // act
        var result = rewriter.Rewrite(sampleObject, _context);

        // assert
        Assert.NotNull(result);
        var resultObj = result as JsonObject;
        Assert.NotNull(resultObj);
        Assert.Equal(42, resultObj["number"]?.GetValue<int>());
        Assert.True(resultObj["boolean"]?.GetValue<bool>());
        Assert.Equal(3.14, resultObj["decimal"]?.GetValue<double>());
    }

    [Fact]
    public void Rewrite_DeeplyNestedWithNulls_DoesNotThrow()
    {
        // arrange
        var sampleObject = new JsonObject()
        {
            ["level1"] = new JsonObject()
            {
                ["level2"] = new JsonObject()
                {
                    ["level3"] = null,
                    ["items"] = new JsonArray(
                        null,
                        new JsonObject()
                        {
                            ["nested"] = null,
                            ["path"] = "$solution:/deep/path"
                        }
                    )
                },
                ["nullHere"] = null
            },
            ["topLevelNull"] = null
        };
        var rewriter = new MagicPathRewriter();

        // act
        var result = rewriter.Rewrite(sampleObject, _context);

        // assert
        Assert.NotNull(result);
    }
}