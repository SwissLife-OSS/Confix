using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool.Reporting;

public sealed class ProjectReportMiddleware : IMiddleware
{
    /// <inheritdoc />
    public async Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        var configuration = context.Features.Get<ConfigurationFeature>();
        configuration.EnsureProject();

        var reports = await CreateReportsAsync(context, context.CancellationToken);

        context.SetOutput(reports);

        await PrintResultAsync(context, reports, context.CancellationToken);

        await next(context);
    }

    private static async Task PrintResultAsync(
        IMiddlewareContext context,
        List<Report> reports,
        CancellationToken ct)
    {
        var formatter = context.Services.GetRequiredService<IOutputFormatter>();
        var formattedOutput = await formatter.FormatAsync(OutputFormat.Json, reports);

        if (context.Parameter.TryGet(ReportOutputFileOption.Instance, out FileInfo file))
        {
            Reporting.Log.ReportOutputFileWasWritten(App.Log, file.FullName);
            await File.WriteAllTextAsync(file.FullName, formattedOutput, ct);
        }
        else
        {
            await using var _ = await context.Status.PauseAsync(ct);
            context.Console.WriteJson(formattedOutput);
        }
    }

    private static async Task<List<Report>> CreateReportsAsync(
        IMiddlewareContext context,
        CancellationToken ct)
    {
        var log = context.Logger;

        var configuration = context.Features.Get<ConfigurationFeature>();
        var fileFeature = context.Features.Get<ConfigurationFileFeature>();
        var envFeature = context.Features.Get<EnvironmentFeature>();

        var project = configuration.EnsureProject();

        var commitReport = await GetCommitReportAsync(project.Directory!, ct);
        Reporting.Log.CommitReported(log, commitReport);

        var repositoryReport = await GetRepositoryReport(project.Directory!, ct);
        Reporting.Log.RepositoryReported(log, repositoryReport);

        var solutionReport = GetSolutionReport(configuration.Solution, repositoryReport);
        Reporting.Log.SolutionReported(log, solutionReport);

        var projectReport = GetProjectReport(project, repositoryReport);
        Reporting.Log.ProjectReported(log, projectReport);

        await PrepareInputFileAsync(context, fileFeature, ct);

        var reports = new List<Report>();

        foreach (var file in fileFeature.Files)
        {
            var report = new Report(
                file.InputFile.FullName,
                envFeature.ActiveEnvironment.Name,
                DateTimeOffset.UtcNow,
                projectReport,
                solutionReport,
                repositoryReport,
                commitReport);

            Reporting.Log.ReportCreated(log, report);

            reports.Add(report);
        }

        return reports;
    }

    private static async Task PrepareInputFileAsync(
        IMiddlewareContext context,
        ConfigurationFileFeature fileFeature,
        CancellationToken ct)
    {
        if (!context.Parameter.TryGet(ReportInputFileOption.Instance, out FileInfo inputFile))
        {
            return;
        }

        if (fileFeature.Files.Count == 0)
        {
            return;
        }

        if (fileFeature.Files.Count > 1)
        {
            throw new ExitException(
                "Cannot create report for multiple configuration files when a single configuration file is specified")
            {
                Help =
                    $"Please run {"confix project report".AsCommand()} without the {"--input-file".AsCommand()} option"
            };
        }

        fileFeature.Files[0].OutputFile = inputFile;
        var content = await File.ReadAllTextAsync(inputFile.FullName, ct);
        fileFeature.Files[0].Content = JsonNode.Parse(content);
    }

    private static async Task<CommitReport> GetCommitReportAsync(
        FileSystemInfo directory,
        CancellationToken ct)
    {
        var branch = await GitHelpers.GetBranchAsync(new(directory.FullName, null), ct);
        if (branch is null)
        {
            throw new ExitException(
                $"Could not determine branch for directory {directory.FullName}")
            {
                Help = "Are you running this command from a git repository?"
            };
        }

        var tags = await GitHelpers.GetTagsAsync(new(directory.FullName, null), ct);
        if (tags is null)
        {
            throw new ExitException(
                $"Could not determine tags for directory {directory.FullName}")
            {
                Help = "Are you running this command from a git repository?"
            };
        }

        var info = await GitHelpers.GetRepoInfoAsync(new(directory.FullName, null), ct);
        if (info is null)
        {
            throw new ExitException(
                $"Could not determine repository info for directory {directory.FullName}")
            {
                Help = "Are you running this command from a git repository?"
            };
        }

        return new CommitReport(info.Hash, info.Message, info.Author, info.Email, branch, tags);
    }

    private static async Task<RepositoryReport> GetRepositoryReport(
        FileSystemInfo directory,
        CancellationToken ct)
    {
        var path = await GitHelpers.GetRootAsync(new(directory.FullName, null), ct);
        if (path is null)
        {
            throw new ExitException(
                $"Could not determine repository root for directory {directory.FullName}")
            {
                Help = "Are you running this command from a git repository?"
            };
        }

        var name = Path.GetDirectoryName(path);
        if (name is null)
        {
            throw new ExitException(
                $"Could not determine repository name for directory {path}")
            {
                Help = "Are you running this command from a git repository?"
            };
        }

        var originUrl = await GitHelpers.GetOriginUrlAsync(new(directory.FullName, null), ct);
        if (originUrl is null)
        {
            throw new ExitException(
                $"Could not determine repository origin url for directory {directory.FullName}")
            {
                Help = "Are you running this command from a git repository?"
            };
        }

        return new RepositoryReport(name, originUrl, path);
    }

    private static SolutionReport? GetSolutionReport(
        SolutionDefinition? solution,
        RepositoryReport repositoryReport)
    {
        if (solution is not { Directory: { } directory })
        {
            return null;
        }

        var path = Path.GetRelativePath(repositoryReport.Path, directory.FullName);

        return new SolutionReport(directory.Name, path);
    }

    private static ProjectReport GetProjectReport(
        ProjectDefinition project,
        RepositoryReport repositoryReport)
    {
        var path = Path.GetRelativePath(repositoryReport.Path, project.Directory!.FullName);

        return new ProjectReport(project.Name, path);
    }
}
