using System.Diagnostics;
using Confix.Tool.Commands.Logging;
using Spectre.Console;

namespace Confix.Utilities;

public static class GitHelpers
{
    public static async Task SparseCheckoutAsync(
        GitSparseCheckoutConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var arguments = new List<string>()
        {
            "clone", "--no-checkout"
        };

        if (configuration.Arguments?.Length > 0)
        {
            arguments.AddRange(configuration.Arguments);
        }

        arguments.Add(configuration.RepositoryUrl);
        arguments.Add($"\"{configuration.Location}\"");

        try
        {
            App.Log.GitSparseCheckoutStarted(configuration.RepositoryUrl, configuration.Location);

            var output = await ExecuteAsync(arguments, cancellationToken);

            App.Log.GitSparseCheckoutOutput(output);
        }
        catch (Exception ex)
        {
            App.Log.GitSparseCheckoutFailed(ex);
        }
    }

    public static async Task<string> ShowRefsAsync(
        GitShowRefsConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var arguments = new List<string>()
        {
            "-C",
            $"\"{configuration.Location}\"",
            "show-ref"
        };

        if (configuration.Arguments?.Length > 0)
        {
            arguments.AddRange(configuration.Arguments);
        }

        try
        {
            App.Log.GitShowRefsStarted(configuration.Location);

            var output = await ExecuteAsync(arguments, cancellationToken);

            App.Log.GitShowRefsOutput(output);
            return output;
        }
        catch (Exception ex)
        {
            App.Log.GitShowRefsFailed(ex);
        }

        return string.Empty;
    }

    public static async Task CheckoutAsync(
        GitCheckoutConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var arguments = new List<string>()
        {
            "-C",
            $"\"{configuration.Location}\"",
            "checkout",
            configuration.Ref
        };
        if (configuration.Arguments?.Length > 0)
        {
            arguments.AddRange(configuration.Arguments);
        }

        try
        {
            App.Log.GitCheckoutStarted(configuration.Ref);

            var output = await ExecuteAsync(arguments, cancellationToken);

            App.Log.GitCheckoutOutput(output);
        }
        catch (Exception ex)
        {
            App.Log.GitCheckoutFailed(ex);
        }
    }

    public static async Task CloneAsync(
        GitCloneConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var arguments = new List<string>()
        {
            "clone"
        };
        if (configuration.Arguments?.Length > 0)
        {
            arguments.AddRange(configuration.Arguments);
        }

        arguments.Add(configuration.RepositoryUrl);
        arguments.Add($"\"{configuration.Location}\"");

        try
        {
            App.Log.GitCloneStarted(configuration.RepositoryUrl, configuration.Location);

            var output = await ExecuteAsync(arguments, cancellationToken);

            App.Log.GitCloneOutput(output);
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
        var arguments = new List<string>()
        {
            "-C",
            $"\"{configuration.Location}\"",
            "pull"
        };
        if (configuration.Arguments?.Length > 0)
        {
            arguments.AddRange(configuration.Arguments);
        }

        try
        {
            App.Log.GitPullStarted(configuration.Location);

            var output = await ExecuteAsync(arguments, cancellationToken);

            App.Log.GitPullOutput(output);
        }
        catch (Exception ex)
        {
            App.Log.GitPullFailed(ex);
        }
    }

    public static async Task AddAsync(
        GitAddConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var arguments = new List<string>()
        {
            "-C",
            $"\"{configuration.Location}\"",
            "add", "*"
        };
        if (configuration.Arguments?.Length > 0)
        {
            arguments.AddRange(configuration.Arguments);
        }

        try
        {
            App.Log.GitAddStarted(configuration.Location);

            var output = await ExecuteAsync(arguments, cancellationToken);

            App.Log.GitAddOutput(output);
        }
        catch (Exception ex)
        {
            App.Log.GitAddFailed(ex);
        }
    }

    public static async Task CommitAsync(
        GitCommitConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var arguments = new List<string>()
        {
            "-C",
            $"\"{configuration.Location}\"",
            "commit",
            "-m",
            $"\"{configuration.Message}\""
        };
        if (configuration.Arguments?.Length > 0)
        {
            arguments.AddRange(configuration.Arguments);
        }

        try
        {
            App.Log.GitCommitStarted(configuration.Location);

            var output = await ExecuteAsync(arguments, cancellationToken);

            App.Log.GitCommitOutput(output);
        }
        catch (Exception ex)
        {
            App.Log.GitCommitFailed(ex);
        }
    }

    public static async Task PushAsync(
        GitPushConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var arguments = new List<string>()
        {
            "-C",
            $"\"{configuration.Location}\"",
            "push"
        };
        if (configuration.Arguments?.Length > 0)
        {
            arguments.AddRange(configuration.Arguments);
        }

        try
        {
            App.Log.GitPushStarted(configuration.Location);

            var output = await ExecuteAsync(arguments, cancellationToken);

            App.Log.GitPushOutput(output);
        }
        catch (Exception ex)
        {
            App.Log.GitPushFailed(ex);
        }
    }

    private static async Task<string> ExecuteAsync(
        IEnumerable<string> arguments,
        CancellationToken cancellationToken)
    {
        var argumentsString = string.Join(" ", arguments);
        using Process process = new()
        {
            StartInfo = new()
            {
                FileName = "git",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments = argumentsString
            }
        };
        
        App.Log.Debug($"Executing `git {argumentsString}`");

        process.Start();

        await process.WaitForExitAsync(cancellationToken);

        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);

        process.EnsureExitCode();

        return output;
    }
}

public record GitCloneConfiguration(
    string RepositoryUrl,
    string Location,
    string[]? Arguments);

public record GitCheckoutConfiguration(
    string Location,
    string Ref,
    string[]? Arguments);

public record GitShowRefsConfiguration(
    string Location,
    string[]? Arguments);

public record GitSparseCheckoutConfiguration(
    string RepositoryUrl,
    string Location,
    string[]? Arguments);

public record GitPullConfiguration(
    string Location,
    string[]? Arguments);

public record GitAddConfiguration(
    string Location,
    string[]? Arguments);

public record GitCommitConfiguration(
    string Location,
    string Message,
    string[]? Arguments);

public record GitPushConfiguration(
    string Location,
    string[]? Arguments);

file static class LogExtensions
{
    public static void EnsureExitCode(this Process process)
    {
        if (process.ExitCode != 0)
        {
            throw new Exception($"Process exited with code {process.ExitCode}");
        }
    }

    public static void GitCloneStarted(
        this IConsoleLogger log,
        string repositoryUrl,
        string location)
    {
        log.Debug($"Cloning {repositoryUrl} to {location}");
    }

    public static void GitCloneOutput(this IConsoleLogger log, string output)
    {
        log.Debug(output.EscapeMarkup());
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
        log.Debug(output.EscapeMarkup());
    }

    public static void GitPullFailed(this IConsoleLogger log, Exception ex)
    {
        log.Exception("Git pull failed", ex);
    }

    public static void GitAddStarted(this IConsoleLogger log, string location)
    {
        log.Debug($"Adding files from {location}");
    }

    public static void GitAddOutput(this IConsoleLogger log, string output)
    {
        log.Debug(output.EscapeMarkup());
    }

    public static void GitAddFailed(this IConsoleLogger log, Exception ex)
    {
        log.Exception("Git add failed", ex);
    }

    public static void GitCommitStarted(this IConsoleLogger log, string location)
    {
        log.Debug($"Committing files from {location.EscapeMarkup()}");
    }

    public static void GitCommitOutput(this IConsoleLogger log, string output)
    {
        log.Debug(output.EscapeMarkup());
    }

    public static void GitCommitFailed(this IConsoleLogger log, Exception ex)
    {
        log.Exception("Git commit failed", ex);
    }

    public static void GitPushStarted(this IConsoleLogger log, string location)
    {
        log.Debug($"Pushing files from {location.EscapeMarkup()}");
    }

    public static void GitPushOutput(this IConsoleLogger log, string output)
    {
        log.Debug(output.EscapeMarkup());
    }

    public static void GitPushFailed(this IConsoleLogger log, Exception ex)
    {
        log.Exception("Git push failed", ex);
    }

    public static void GitSparseCheckoutStarted(
        this IConsoleLogger log,
        string repositoryUrl,
        string location)
    {
        log.Debug($"Sparse checkout {repositoryUrl} to {location}");
    }

    public static void GitSparseCheckoutOutput(this IConsoleLogger log, string output)
    {
        log.Debug(output.EscapeMarkup());
    }

    public static void GitSparseCheckoutFailed(this IConsoleLogger log, Exception ex)
    {
        log.Exception("Git sparse checkout failed", ex);
    }

    public static void GitShowRefsStarted(
        this IConsoleLogger log,
        string location)
    {
        log.Debug($"Show refs in {location}");
    }

    public static void GitShowRefsOutput(this IConsoleLogger log, string output)
    {
        log.Debug(output.EscapeMarkup());
    }

    public static void GitShowRefsFailed(this IConsoleLogger log, Exception ex)
    {
        log.Exception("Git show refs failed", ex);
    }

    public static void GitCheckoutStarted(this IConsoleLogger log, string @ref)
    {
        log.Debug($"Checkout {@ref}");
    }

    public static void GitCheckoutOutput(this IConsoleLogger log, string output)
    {
        log.Debug(output.EscapeMarkup());
    }

    public static void GitCheckoutFailed(this IConsoleLogger log, Exception ex)
    {
        log.Exception("Git checkout failed", ex);
    }
}
