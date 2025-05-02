using Confix.Tool;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Entities.Components.Git;
using Moq;

public class GitUrlTests
{
    [Theory]
    [InlineData("https://github.com/org/repo.git", "user", "token", "https://user:token@github.com/org/repo.git")]
    [InlineData("https://github.com/org/repo.git", null, null, "https://github.com/org/repo.git")]
    [InlineData("https://github.com/org/repo.git", "", "", "https://github.com/org/repo.git")]
    [InlineData("http://github.com/org/repo.git", "user", "token", "http://github.com/org/repo.git")]
    [InlineData("https://github.com/org/repo.git", "user", null, "https://github.com/org/repo.git")]
    [InlineData("https://github.com/org/repo.git", null, "token", "https://github.com/org/repo.git")]
    public void GitUrl_Create_WorksAsExpected(string url, object username, object token, string expected)
    {
        // Arrange
        var parameters = new Mock<IParameterCollection>();
        parameters.Setup(p => p.TryGet(It.IsAny<GitUsernameOptions>(), out username)).Returns(true);
        parameters.Setup(p => p.TryGet(It.IsAny<GitTokenOptions>(), out token)).Returns(true);
        
        // Act
        var result = GitUrl.Create(url, parameters.Object);

        // Assert
        Assert.Equal(expected, result);
    }
}
