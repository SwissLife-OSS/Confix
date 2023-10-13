using Confix.Inputs;
using Confix.Tool.Commands.Temp;
using Confix.Tool.Schema;
using Confix.Utilities;
using Moq;

namespace Confix.Entities.Component.Configuration.Middlewares;

public class ProjectReportTests : IAsyncLifetime
{
    private readonly Mock<IGitService> _git;
    private readonly TestConfixCommandline _cli;

    public ProjectReportTests()
    {
        _git = new Mock<IGitService>();
        _cli = new TestConfixCommandline(x => x.AddTestService<IGitService>(_ => _git.Object));
    }

    [Fact]
    public async Task Should_OutputReportToCli_When_NoFileSpecified()
    {
        // arrange
        _cli.Directories.Home.CreateConfixRc(_confixRc);
        _cli.Directories.Content
            .CreateConfixProject(path: $"src/project/{FileNames.ConfixProject}");
        _cli.Directories.Content.CreateConfixSolution();
        _cli.ExecutionContext = _cli.ExecutionContext with
        {
            CurrentDirectory = _cli.Directories.Content
                .Append("src")
                .Append("project")
        };

        _git.SetupGitRoot(_cli.Directories.Content);
        _git.SetupGitBranch();
        _git.SetupGitTags();
        _git.SetupGitRepoInfo();

        // act
        await _cli.RunAsync("project report");

        // assert
        SnapshotBuilder.New().AddOutput(_cli).MatchSnapshot();
    }

    /// <inheritdoc />
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DisposeAsync()
    {
        _cli.Dispose();
        return Task.CompletedTask;
    }

    private const string _confixRc = """
            {
                "isRoot": true,
                "component":
                {
                     "inputs": [
                          {
                            "type": "graphql"
                          },
                          {
                            "type": "dotnet"
                          }
                        ]
                },
                "project": {
                     "componentProviders": [
                          {
                            "name": "dotnet-package",
                            "type": "dotnet-package"
                          }
                        ]
                }
            }
        """;
}

file static class Extensions
{
    public static void SetupGitRoot(this Mock<IGitService> service, FileSystemInfo root)
    {
        service.Setup(
                x => x.GetRootAsync(It.IsAny<GitGetRootConfiguration>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => root.FullName);
    }

    public static void SetupGitBranch(
        this Mock<IGitService> service,
        string branch = "master")
    {
        service.Setup(
                x => x.GetBranchAsync(It.IsAny<GitGetBranchConfiguration>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => branch);
    }

    public static void SetupGitTags(
        this Mock<IGitService> service,
        string[]? tags = default!)
    {
        tags ??= new[]
        {
            "1.0.0",
            "1.1.0"
        };
        service.Setup(
                x => x.GetTagsAsync(It.IsAny<GitGetTagsConfiguration>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => tags);
    }

    public static void SetupGitRepoInfo(
        this Mock<IGitService> service,
        string hash = "1234567890",
        string message = "Initial commit",
        string author = "John Doe",
        string email = "jd@gmail.com")
    {
        service.Setup(
                x => x.GetRepoInfoAsync(It.IsAny<GitGetRepoInfoConfiguration>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new GitGetRepoInfoResult(hash, message, author, email));
    }
}
