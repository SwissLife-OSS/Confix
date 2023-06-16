using System.Text.Json.Nodes;
using Confix.Tool.Middlewares;
using Json.More;
using Snapshooter.Xunit;

namespace ConfiX.Entities.Component.Configuration.Middlewares;

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
        Snapshot.Match(result.ToJsonString(new() { WriteIndented = true }));
    }
}