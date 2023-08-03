
using System.Diagnostics;
using Confix.Tool.Commands.Logging;

namespace Confix.Variables;

public static class GitHelpers
{
    public static async Task CloneAsync(
        GitCloneConfiguration configuration,
        CancellationToken cancellationToken)
    {
        List<string> arguments = new()
        {
            "clone"
        };
        if (configuration.Arguments?.Length > 0)
        {
            arguments.AddRange(configuration.Arguments);
        }
        arguments.Add(configuration.RepositoryUrl);
        arguments.Add($"\"{configuration.Location}\"");

        using Process process = new()
        {
            StartInfo = new()
            {
                FileName = "git",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments = string.Join(" ", arguments)
            }
        };

        try
        {
            App.Log.GitCloneStarted(configuration.RepositoryUrl, configuration.Location);
            process.Start();

            await process.WaitForExitAsync(cancellationToken);

            string output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            App.Log.GitCloneOutput(output);

            process.EnsureExitCode();
        }
        catch (Exception ex)
        {
            App.Log.GitCloneFailed(ex);
        }
    }
    
    public static async Task PullAsync(
        GitPullConfiguration configuration,
        CancellationToken cancellationToken)
    {
        List<string> arguments = new()
        {
            "-C",
            $"\"{configuration.Location}\"",
            "pull"
        };
        if (configuration.Arguments?.Length > 0)
        {
            arguments.AddRange(configuration.Arguments);
        }

        using Process process = new()
        {
            StartInfo = new()
            {
                FileName = "git",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments = string.Join(" ", arguments)
            }
        };

        try
        {
            App.Log.GitPullStarted(configuration.Location);
            process.Start();

            await process.WaitForExitAsync(cancellationToken);

            string output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            App.Log.GitPullOutput(output);

            process.EnsureExitCode();
        }
        catch (Exception ex)
        {
            App.Log.GitPullFailed(ex);
        }
    }
}

public record GitCloneConfiguration(
    string RepositoryUrl,
    string Location,
    string[]? Arguments
);

public record GitPullConfiguration(
    string Location,
    string[]? Arguments
);

file static class LogExtensions
{
    public static void EnsureExitCode(this Process process)
    {
        if (process.ExitCode != 0)
        {
            throw new Exception($"Process exited with code {process.ExitCode}");
        }
    }

    public static void GitCloneStarted(this IConsoleLogger log, string repositoryUrl, string location)
    {
        log.Debug($"Cloning {repositoryUrl} to {location}");
    }

    public static void GitCloneOutput(this IConsoleLogger log, string output)
    {
        log.Debug(output);
    }

    public static void GitCloneFinished(this IConsoleLogger log, int exitCode)
    {
        log.Debug($"Cloning completed with exit code {exitCode}");
    }

    public static void GitCloneFailed(this IConsoleLogger log, Exception ex)
    {
        log.Exception("Git clone failed", ex);
    }
    
    public static void GitPullStarted(this IConsoleLogger log, string location)
    {
        log.Debug($"Pulling repository from {location}");
    }
    
    public static void GitPullOutput(this IConsoleLogger log, string output)
    {
        log.Debug(output);
    }
    
    public static void GitPullFailed(this IConsoleLogger log, Exception ex)
    {
        log.Exception("Git pull failed", ex);
    }
}