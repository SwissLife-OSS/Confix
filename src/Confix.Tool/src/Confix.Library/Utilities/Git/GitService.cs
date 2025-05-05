using System.Diagnostics;
using Confix.Tool.Commands.Logging;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Confix.Utilities;

public sealed class GitService : IGitService
{
    public async Task SparseCheckoutAsync(
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

    public async Task<string> ShowRefsAsync(
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

    public async Task CheckoutAsync(
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

    public async Task CloneAsync(
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

    public async Task PullAsync(
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

    public async Task AddAsync(
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

    public async Task CommitAsync(
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

    public async Task PushAsync(
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

    public async Task<GitGetRepoInfoResult?> GetRepoInfoAsync(
        GitGetRepoInfoConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var arguments = new List<string>()
        {
            "-C",
            $"\"{configuration.Location}\"",
            "log",
            "-1",
            "--decorate=full",
            "--pretty=format:'hash:%H\nmessage:%s\nauthor:%an\nemail:%ae\ntimestamp:%ai'"
        };

        if (configuration.Arguments?.Length > 0)
        {
            arguments.AddRange(configuration.Arguments);
        }

        try
        {
            App.Log.GitGetInfoStarted(configuration.Location);

            var output = await ExecuteAsync(arguments, cancellationToken);

            var lines = output.Split(Environment.NewLine);
            var hash = lines[0].Split(':', 2, StringSplitOptions.TrimEntries)[1];
            var message = lines[1].Split(':', 2, StringSplitOptions.TrimEntries)[1];
            var author = lines[2].Split(':', 2, StringSplitOptions.TrimEntries)[1];
            var email = lines[3].Split(':', 2, StringSplitOptions.TrimEntries)[1];

            App.Log.GitGetInfoOutput(output);
            return new GitGetRepoInfoResult(hash, message, author, email);
        }
        catch (Exception ex)
        {
            App.Log.GitGetInfoFailed(ex);
            return null;
        }
    }

    public async Task<string?> GetBranchAsync(
        GitGetBranchConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var arguments = new List<string>
        {
            "-C",
            $"\"{configuration.Location}\"",
            "rev-parse",
            "--abbrev-ref",
            "HEAD"
        };

        if (configuration.Arguments?.Length > 0)
        {
            arguments.AddRange(configuration.Arguments);
        }

        try
        {
            App.Log.GitGetBranchStarted(configuration.Location);

            var output = await ExecuteAsync(arguments, cancellationToken);

            App.Log.GitGetBranchOutput(output);

            output = output.TrimNewLine();

            return output;
        }
        catch (Exception ex)
        {
            App.Log.GitGetBranchFailed(ex);
            return null;
        }
    }

    public async Task<IReadOnlyList<string>?> GetTagsAsync(
        GitGetTagsConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var arguments = new List<string>()
        {
            "-C",
            $"\"{configuration.Location}\"",
            "tag",
            "--points-at",
            "HEAD"
        };

        if (configuration.Arguments?.Length > 0)
        {
            arguments.AddRange(configuration.Arguments);
        }

        try
        {
            App.Log.GitGetTagStarted(configuration.Location);

            var output = await ExecuteAsync(arguments, cancellationToken);

            App.Log.GitGetTagOutput(output);

            return output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        }
        catch (Exception ex)
        {
            App.Log.GitGetTagFailed(ex);
            return null;
        }
    }

    public async Task<string?> GetRootAsync(
        GitGetRootConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var arguments = new List<string>()
        {
            "-C",
            $"\"{configuration.Location}\"",
            "rev-parse",
            "--show-toplevel"
        };

        if (configuration.Arguments?.Length > 0)
        {
            arguments.AddRange(configuration.Arguments);
        }

        try
        {
            App.Log.GitGetRootStarted(configuration.Location);

            var output = await ExecuteAsync(arguments, cancellationToken);

            App.Log.GitGetRootOutput(output);

            output = output.TrimNewLine();

            return output;
        }
        catch (Exception ex)
        {
            App.Log.GitGetRootFailed(ex);
            return null;
        }
    }

    public async Task<string?> GetOriginUrlAsync(
        GitGetOriginUrlConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var arguments = new List<string>()
        {
            "-C",
            $"\"{configuration.Location}\"",
            "config",
            "--get",
            "remote.origin.url"
        };

        if (configuration.Arguments?.Length > 0)
        {
            arguments.AddRange(configuration.Arguments);
        }

        try
        {
            App.Log.GitGetOriginUrlStarted(configuration.Location);

            var output = await ExecuteAsync(arguments, cancellationToken);

            App.Log.GitGetOriginUrlOutput(output);

            output = output.TrimNewLine();

            return output;
        }
        catch (Exception ex)
        {
            App.Log.GitGetOriginUrlFailed(ex);
            return null;
        }
    }

    private async Task<string> ExecuteAsync(
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

        App.Log.Debug("Executing `git`");
        process.Start();

        await process.WaitForExitAsync(cancellationToken);

        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);

        process.EnsureExitCode();

        return output;
    }
}

file static class Extensions
{
    public static string TrimNewLine(this string value)
    {
        return value.TrimEnd('\r', '\n');
    }
}

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
        log.LogException(ex);
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
        log.LogException(ex);
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
        log.LogException(ex);
        log.Exception("Git checkout failed", ex);
    }

    public static void GitGetInfoStarted(this IConsoleLogger log, string location)
    {
        log.Debug($"Get info in {location}");
    }

    public static void GitGetInfoOutput(this IConsoleLogger log, string output)
    {
        log.Debug(output.EscapeMarkup());
    }

    public static void GitGetInfoFailed(this IConsoleLogger log, Exception ex)
    {
        log.LogException(ex);
        log.Exception("Git get info failed", ex);
    }

    public static void GitGetBranchStarted(this IConsoleLogger log, string location)
    {
        log.Debug($"Get branch in {location}");
    }

    public static void GitGetBranchOutput(this IConsoleLogger log, string output)
    {
        log.Debug(output.EscapeMarkup());
    }

    public static void GitGetBranchFailed(this IConsoleLogger log, Exception ex)
    {
        log.LogException(ex);
        log.Exception("Git get branch failed", ex);
    }

    public static void GitGetTagStarted(this IConsoleLogger log, string location)
    {
        log.Debug($"Get tag in {location}");
    }

    public static void GitGetTagOutput(this IConsoleLogger log, string output)
    {
        log.Debug(output.EscapeMarkup());
    }

    public static void GitGetTagFailed(this IConsoleLogger log, Exception ex)
    {
        log.LogException(ex);
        log.Exception("Git get tag failed", ex);
    }

    public static void GitGetRootStarted(this IConsoleLogger log, string location)
    {
        log.Debug($"Get repository root in {location}");
    }

    public static void GitGetRootOutput(this IConsoleLogger log, string output)
    {
        log.Debug(output.EscapeMarkup());
    }

    public static void GitGetRootFailed(this IConsoleLogger log, Exception ex)
    {
        log.LogException(ex);
        log.Exception("Git get repository root failed", ex);
    }

    public static void GitGetOriginUrlStarted(this IConsoleLogger log, string location)
    {
        log.Debug($"Get origin url in {location}");
    }

    public static void GitGetOriginUrlOutput(this IConsoleLogger log, string output)
    {
        log.Debug(output.EscapeMarkup());
    }

    public static void GitGetOriginUrlFailed(this IConsoleLogger log, Exception ex)
    {
        log.LogException(ex);
        log.Exception("Git get origin url failed", ex);
    }
    
    private static void LogException(
        this IConsoleLogger log,
        Exception ex)
    {
        log.Error(ex.Message);
        if (ex.InnerException != null)
        {
            log.Error(ex.InnerException.Message);
        }
    }
}