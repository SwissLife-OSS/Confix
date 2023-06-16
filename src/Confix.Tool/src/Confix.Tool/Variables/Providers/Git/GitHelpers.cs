using System.Diagnostics;
using Confix.Tool.Commands.Logging;

namespace ConfiX.Variables;

public static class GitHelpers
{
    public static async Task Clone(
        GitCloneConfiguration configuration,
        CancellationToken cancellationToken)
    {
        using Process process = new()
        {
            StartInfo = new()
            {
                FileName = "git",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                ArgumentList = {
                    "clone",
                    $"--depth={configuration.Depth}",
                    $"--branch {configuration.Branch}",
                    configuration.Arguments,
                    configuration.RepositoryUrl,
                    configuration.Location}
            }
        };

        try
        {
            App.Log.GitCloneStarted(configuration.RepositoryUrl);
            process.Start();

            await process.WaitForExitAsync(cancellationToken);

            string output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            App.Log.GitCloneOutput(output);
            App.Log.GitCloneFinished(process.ExitCode);
        }
        catch (Exception ex)
        {
            App.Log.GitCloneFailed(ex);
        }
    }
}

public record GitCloneConfiguration(
    string RepositoryUrl,
    string Location,
    string Branch,
    int Depth,
    string? Arguments
);

file static class LogExtensions
{
    public static void GitCloneStarted(this IConsoleLogger log, string repositoryUrl)
    {
        log.Information($"Cloning {repositoryUrl}...");
    }

    public static void GitCloneOutput(this IConsoleLogger log, string output)
    {
        log.Debug(output);
    }

    public static void GitCloneFinished(this IConsoleLogger log, int exitCode)
    {
        log.Information($"Cloning completed with exit code {exitCode}");
    }

    public static void GitCloneFailed(this IConsoleLogger log, Exception ex)
    {
        log.Exception("Git clone failed", ex);
    }
}