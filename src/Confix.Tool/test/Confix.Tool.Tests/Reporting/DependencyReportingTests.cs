using Confix.Inputs;
using Confix.Tool.Commands.Temp;
using Confix.Tool.Schema;
using Confix.Utilities;
using Moq;

namespace Confix.Reporting;

public class DependencyReportingTests
{
    private readonly Mock<IGitService> _git;
    private readonly TestConfixCommandline _cli;

    public DependencyReportingTests()
    {
        _git = new Mock<IGitService>();
        _cli = new TestConfixCommandline(x => x.AddTestService<IGitService>(_ => _git.Object));
    }

    [Fact]
    public async Task Should_OutputDependency_When_Match()
    {
        // arrange
        _cli.Directories.Home.CreateConfixRc(_confixRc);
        _cli.Directories.Content
            .CreateConfixProject(path: $"src/project/{FileNames.ConfixProject}");
        _cli.Directories.Content.CreateFileInPath("src/project/test.json",
            """
            {
                "test": {
                    "value": "The date is 2023-10-19"
                }
            }
            """
        );
        _cli.Directories.Content.CreateFileInPath(".confix/.schemas/src.project.schema.json", "{}");
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
        _git.SetupOriginUrl();

        // act
        await _cli.RunAsync("project report");

        // assert
        SnapshotBuilder.New()
            .AddOutput(_cli)
            .RemoveDateTimes()
            .MatchSnapshot();
    }

    [Fact]
    public async Task Should_OutputMultipleDependency_When_Match()
    {
        // arrange
        _cli.Directories.Home.CreateConfixRc(_confixRc);
        _cli.Directories.Content
            .CreateConfixProject(path: $"src/project/{FileNames.ConfixProject}");
        _cli.Directories.Content.CreateFileInPath("src/project/test.json",
            """
            {
                "nested": {
                    "first": "The date is 2023-10-19"
                },
                "arr": [
                    {
                        "second": "The date is 2023-10-19"
                    },
                    {
                        "deep": {
                            "third": "The date is 2023-10-19"
                        }
                    }
                ]
            }
            """
        );
        _cli.Directories.Content.CreateFileInPath(".confix/.schemas/src.project.schema.json", "{}");
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
        _git.SetupOriginUrl();

        // act
        await _cli.RunAsync("project report");

        // assert
        SnapshotBuilder.New()
            .AddOutput(_cli)
            .RemoveDateTimes()
            .MatchSnapshot();
    }

    [Fact]
    public async Task Should_NotError_When_NotRestored()
    {
        // arrange
        _cli.Directories.Home.CreateConfixRc(_confixRc);
        _cli.Directories.Content
            .CreateConfixProject(path: $"src/project/{FileNames.ConfixProject}");
        _cli.Directories.Content.CreateFileInPath("src/project/test.json",
            """
            {
                "test": {
                    "value": "The date is 2023-10-19"
                }
            }
            """
        );
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
        _git.SetupOriginUrl();

        // act
        await _cli.RunAsync("project report");

        // assert
        SnapshotBuilder.New()
            .AddOutput(_cli)
            .RemoveDateTimes()
            .MatchSnapshot();
    }

    private const string _confixRc = """
            {
                "isRoot": true,
                "component":
                {
                     "inputs": [
                          {
                            "type": "graphql"
                          }
                        ]
                },
                "project": {
                     "componentProviders": [],
                     "configurationFiles": ["$project:/test.json"]
                },
                "reporting": {
                    "dependencies": {
                        "providers": [
                            {
                                "kind": "example",
                                "type": "regex",
                                "regex": "(?<year>\\d{4})-(\\d{2})-(?<day>\\d{2})(?:\\s*)?"
                            }
                        ]
                    }
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

    public static void SetupOriginUrl(
        this Mock<IGitService> service,
        string url = "https://foobar.com")
        => service.Setup(
                x => x.GetOriginUrlAsync(
                    It.IsAny<GitGetOriginUrlConfiguration>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => url);
}
