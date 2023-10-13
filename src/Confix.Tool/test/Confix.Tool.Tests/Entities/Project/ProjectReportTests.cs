using Confix.Inputs;
using Confix.Tool.Commands.Temp;
using Confix.Tool.Schema;
using Confix.Utilities;
using Moq;

namespace Confix.Entities.Component.Configuration.Middlewares;

public sealed class ProjectReportTests : IAsyncLifetime
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
        _cli.Directories.Content.CreateFileInPath("src/project/test.json", "{}");
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
    public async Task Should_OutputReportToFile_When_Specified()
    {
        // arrange
        _cli.Directories.Home.CreateConfixRc(_confixRc);
        _cli.Directories.Content
            .CreateConfixProject(path: $"src/project/{FileNames.ConfixProject}");
        _cli.Directories.Content.CreateFileInPath("src/project/test.json", "{}");
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
        var outFile = _cli.Directories.Content
            .Append("src")
            .Append("project")
            .AppendFile("report.json");

        // act
        await _cli.RunAsync($"project report --output-file {outFile.FullName}");

        // assert
        SnapshotBuilder.New()
            .AddOutput(_cli)
            .AddFile(outFile)
            .RemoveDateTimes()
            .MatchSnapshot();
    }

    [Fact]
    public async Task Should_SkipInitialization_When_InputFile()
    {
        // arrange
        _cli.Directories.Home.CreateConfixRc(_confixRc);
        _cli.Directories.Content
            .CreateConfixProject(path: $"src/project/{FileNames.ConfixProject}");
        _cli.Directories.Content.CreateFileInPath("src/project/test.json", "{}");
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

        var inFile = _cli.Directories.Content
            .Append("src")
            .Append("project")
            .AppendFile("test.json");

        var outFile = _cli.Directories.Content
            .Append("src")
            .Append("project")
            .AppendFile("report.json");

        // act
        await _cli.RunAsync(
            $"project report --output-file {outFile.FullName} --input-file {inFile.FullName}");

        // assert
        SnapshotBuilder.New()
            .AddOutput(_cli)
            .AddFile(outFile)
            .RemoveDateTimes()
            .MatchSnapshot();
    }

    [Fact]
    public async Task Should_ErrorWhen_MultipleConfigurationFile_But_InputFileSpecified()
    {
        // arrange

        const string confixRc = """
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
                         "configurationFiles": ["$project:/test2.json", "$project:/test.json"]
                    }
                }
            """;

        _cli.Directories.Home.CreateConfixRc(confixRc);
        _cli.Directories.Content
            .CreateConfixProject(path: $"src/project/{FileNames.ConfixProject}");
        _cli.Directories.Content.CreateFileInPath("src/project/test.json", "{}");
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

        var inFile = _cli.Directories.Content
            .Append("src")
            .Append("project")
            .AppendFile("test.json");

        var outFile = _cli.Directories.Content
            .Append("src")
            .Append("project")
            .AppendFile("report.json");

        // act
        await _cli.RunAsync(
            $"project report --output-file {outFile.FullName} --input-file {inFile.FullName}");

        // assert
        SnapshotBuilder.New()
            .AddOutput(_cli)
            .RemoveDateTimes()
            .MatchSnapshot();
    }

    [Fact]
    public async Task Should_ErrorWhen_GetGitRoot_Fails()
    {
        // arrange
        _cli.Directories.Home.CreateConfixRc(_confixRc);
        _cli.Directories.Content
            .CreateConfixProject(path: $"src/project/{FileNames.ConfixProject}");
        _cli.Directories.Content.CreateFileInPath("src/project/test.json", "{}");
        _cli.Directories.Content.CreateConfixSolution();
        _cli.ExecutionContext = _cli.ExecutionContext with
        {
            CurrentDirectory = _cli.Directories.Content
                .Append("src")
                .Append("project")
        };

        _git.SetupGitBranch();
        _git.SetupGitTags();
        _git.SetupGitRepoInfo();
        _git.SetupOriginUrl();

        // act
        await _cli.RunAsync("project report");

        // assert
        SnapshotBuilder.New().AddOutput(_cli).MatchSnapshot();
    }

    [Fact]
    public async Task Should_ErrorWhen_GetGitBranch_Fails()
    {
        // arrange
        _cli.Directories.Home.CreateConfixRc(_confixRc);
        _cli.Directories.Content
            .CreateConfixProject(path: $"src/project/{FileNames.ConfixProject}");
        _cli.Directories.Content.CreateFileInPath("src/project/test.json", "{}");
        _cli.Directories.Content.CreateConfixSolution();
        _cli.ExecutionContext = _cli.ExecutionContext with
        {
            CurrentDirectory = _cli.Directories.Content
                .Append("src")
                .Append("project")
        };

        _git.SetupGitRoot(_cli.Directories.Content);
        _git.SetupGitTags();
        _git.SetupGitRepoInfo();
        _git.SetupOriginUrl();

        // act
        await _cli.RunAsync("project report");

        // assert
        SnapshotBuilder.New().AddOutput(_cli).MatchSnapshot();
    }

    [Fact]
    public async Task Should_ErrorWhen_GetGitTags_Fails()
    {
        // arrange
        _cli.Directories.Home.CreateConfixRc(_confixRc);
        _cli.Directories.Content
            .CreateConfixProject(path: $"src/project/{FileNames.ConfixProject}");
        _cli.Directories.Content.CreateFileInPath("src/project/test.json", "{}");
        _cli.Directories.Content.CreateConfixSolution();
        _cli.ExecutionContext = _cli.ExecutionContext with
        {
            CurrentDirectory = _cli.Directories.Content
                .Append("src")
                .Append("project")
        };

        _git.SetupGitRoot(_cli.Directories.Content);
        _git.SetupGitBranch();
        _git.SetupGitRepoInfo();
        _git.SetupOriginUrl();

        // act
        await _cli.RunAsync("project report");

        // assert
        SnapshotBuilder.New().AddOutput(_cli).MatchSnapshot();
    }

    [Fact]
    public async Task Should_ErrorWhen_GetGitRepoInfo_Fails()
    {
        // arrange
        _cli.Directories.Home.CreateConfixRc(_confixRc);
        _cli.Directories.Content
            .CreateConfixProject(path: $"src/project/{FileNames.ConfixProject}");
        _cli.Directories.Content.CreateFileInPath("src/project/test.json", "{}");
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
        _git.SetupOriginUrl();

        // act
        await _cli.RunAsync("project report");

        // assert
        SnapshotBuilder.New().AddOutput(_cli).MatchSnapshot();
    }

    [Fact]
    public async Task Should_ErrorWhen_GetGitOriginUrl_Fails()
    {
        // arrange
        _cli.Directories.Home.CreateConfixRc(_confixRc);
        _cli.Directories.Content
            .CreateConfixProject(path: $"src/project/{FileNames.ConfixProject}");
        _cli.Directories.Content.CreateFileInPath("src/project/test.json", "{}");
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
                          }
                        ]
                },
                "project": {
                     "componentProviders": [],
                     "configurationFiles": ["$project:/test.json"]
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
