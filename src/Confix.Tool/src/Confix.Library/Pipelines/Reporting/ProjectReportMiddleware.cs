using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Middlewares;
using Confix.Tool.Schema;
using Confix.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool.Reporting;

public sealed class ProjectReportMiddleware : IMiddleware
{
    private readonly IGitService _git;

    public ProjectReportMiddleware(IGitService git)
    {
        _git = git;
    }

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

    private async Task PrintResultAsync(
        IMiddlewareContext context,
        List<Report> reports,
        CancellationToken ct)
    {
        var formatter = context.Services.GetRequiredService<IOutputFormatter>();
        var formattedOutput = await formatter.FormatAsync(OutputFormat.Json, reports);

        if (context.Parameter.TryGet(ReportOutputFileOption.Instance, out FileInfo file))
        {
            App.Log.ReportOutputFileWasWritten(file.FullName);
            await File.WriteAllTextAsync(file.FullName, formattedOutput, ct);
        }
        else
        {
            await using var _ = await context.Status.PauseAsync(ct);
            context.Logger.WriteJson(formattedOutput);
        }
    }

    private async Task<List<Report>> CreateReportsAsync(
        IMiddlewareContext context,
        CancellationToken ct)
    {
        var log = context.Logger;

        var configuration = context.Features.Get<ConfigurationFeature>();
        var fileFeature = context.Features.Get<ConfigurationFileFeature>();
        var envFeature = context.Features.Get<EnvironmentFeature>();

        var project = configuration.EnsureProject();

        var commitReport = await GetCommitReportAsync(project.Directory!, ct);
        log.CommitReported(commitReport);

        var repositoryReport = await GetRepositoryReport(project.Directory!, ct);
        log.RepositoryReported(repositoryReport);

        var solutionReport = GetSolutionReport(configuration.Solution, repositoryReport);
        log.SolutionReported(solutionReport);

        var projectReport = GetProjectReport(project, repositoryReport);
        log.ProjectReported(projectReport);

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

            log.ReportCreated(report);

            reports.Add(report);
        }

        return reports;
    }

    private async Task<CommitReport> GetCommitReportAsync(
        FileSystemInfo directory,
        CancellationToken ct)
    {
        var branch = await _git.GetBranchAsync(new(directory.FullName, null), ct);
        if (branch is null)
        {
            throw new ExitException(
                $"Could not determine branch for directory {directory.FullName}")
            {
                Help = "Are you running this command from a git repository?"
            };
        }

        var tags = await _git.GetTagsAsync(new(directory.FullName, null), ct);
        if (tags is null)
        {
            throw new ExitException(
                $"Could not determine tags for directory {directory.FullName}")
            {
                Help = "Are you running this command from a git repository?"
            };
        }

        var info = await _git.GetRepoInfoAsync(new(directory.FullName, null), ct);
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

    private async Task<RepositoryReport> GetRepositoryReport(
        FileSystemInfo directory,
        CancellationToken ct)
    {
        var path = await _git.GetRootAsync(new(directory.FullName, null), ct);
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

        var originUrl = await _git.GetOriginUrlAsync(new(directory.FullName, null), ct);
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

file static class Log
{
    public static void CommitReported(
        this IConsoleLogger console,
        CommitReport report)
    {
        console.Debug(
            $"Commit: {report.Hash} by {report.Author} on {report.Branch} [dim]{report.Message}[/]");
    }

    public static void RepositoryReported(
        this IConsoleLogger console,
        RepositoryReport report)
    {
        console.Debug(
            $"Repository: {report.Name} [dim]{report.Path}[/]");
    }

    public static void SolutionReported(
        this IConsoleLogger console,
        SolutionReport? report)
    {
        if (report is null)
        {
            console.Debug("Solution: [dim]None[/]");
            return;
        }

        console.Debug(
            $"Solution: {report.Name} [dim]{report.Path}[/]");
    }

    public static void ProjectReported(
        this IConsoleLogger console,
        ProjectReport report)
    {
        console.Debug(
            $"Project: {report.Name} [dim]{report.Path}[/]");
    }

    public static void ReportCreated(
        this IConsoleLogger console,
        Report report)
    {
        console.Debug(
            $"Report: {report.ConfigurationPath} [dim]{report.Environment}[/]");
    }

    public static void ReportOutputFileWasWritten(
        this IConsoleLogger console,
        string path)
    {
        var fileName = Path.GetFileName(path);
        console.Success($"Report was written to {fileName.ToLink(path)} [dim]{path}[/]");
    }
}
