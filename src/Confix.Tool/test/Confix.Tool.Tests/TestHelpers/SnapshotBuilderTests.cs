namespace Confix.Inputs;

public class SnapshotBuilderTests
{
    [Theory]
    [InlineData(
        @"D:\file-location\foo\bar https://example.com/schema",
        "/file-location/foo/bar https://example.com/schema")]
    [InlineData(
        """{"path":"D:\\file-location\\foo\\bar"}""",
        """{"path":"/file-location/foo/bar"}""")]
    public void NormalizePaths_NormalizesWindowsPaths(
        string content,
        string expected)
    {
        var result = SnapshotBuilder.NormalizePaths(content);

        Assert.Equal(expected, result);
    }
}
