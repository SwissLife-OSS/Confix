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
}