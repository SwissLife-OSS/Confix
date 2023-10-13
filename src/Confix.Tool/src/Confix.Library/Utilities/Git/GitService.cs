using System.Diagnostics;
using Confix.Tool.Commands.Logging;

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
            LogExtensions.GitSparseCheckoutStarted(App.Log, configuration.RepositoryUrl, configuration.Location);

            var output = await ExecuteAsync(arguments, cancellationToken);

            LogExtensions.GitSparseCheckoutOutput(App.Log, output);
        }
        catch (Exception ex)
        {
            LogExtensions.GitSparseCheckoutFailed(App.Log, ex);
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
            LogExtensions.GitShowRefsStarted(App.Log, configuration.Location);

            var output = await ExecuteAsync(arguments, cancellationToken);

            LogExtensions.GitShowRefsOutput(App.Log, output);
            return output;
        }
        catch (Exception ex)
        {
            LogExtensions.GitShowRefsFailed(App.Log, ex);
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
            LogExtensions.GitCheckoutStarted(App.Log, configuration.Ref);

            var output = await ExecuteAsync(arguments, cancellationToken);

            LogExtensions.GitCheckoutOutput(App.Log, output);
        }
        catch (Exception ex)
        {
            LogExtensions.GitCheckoutFailed(App.Log, ex);
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
            LogExtensions.GitCloneStarted(App.Log, configuration.RepositoryUrl, configuration.Location);

            var output = await ExecuteAsync(arguments, cancellationToken);

            LogExtensions.GitCloneOutput(App.Log, output);
        }
        catch (Exception ex)
        {
            LogExtensions.GitCloneFailed(App.Log, ex);
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
            LogExtensions.GitPullStarted(App.Log, configuration.Location);

            var output = await ExecuteAsync(arguments, cancellationToken);

            LogExtensions.GitPullOutput(App.Log, output);
        }
        catch (Exception ex)
        {
            LogExtensions.GitPullFailed(App.Log, ex);
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
            LogExtensions.GitAddStarted(App.Log, configuration.Location);

            var output = await ExecuteAsync(arguments, cancellationToken);

            LogExtensions.GitAddOutput(App.Log, output);
        }
        catch (Exception ex)
        {
            LogExtensions.GitAddFailed(App.Log, ex);
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
            LogExtensions.GitCommitStarted(App.Log, configuration.Location);

            var output = await ExecuteAsync(arguments, cancellationToken);

            LogExtensions.GitCommitOutput(App.Log, output);
        }
        catch (Exception ex)
        {
            LogExtensions.GitCommitFailed(App.Log, ex);
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
            LogExtensions.GitPushStarted(App.Log, configuration.Location);

            var output = await ExecuteAsync(arguments, cancellationToken);

            LogExtensions.GitPushOutput(App.Log, output);
        }
        catch (Exception ex)
        {
            LogExtensions.GitPushFailed(App.Log, ex);
        }
    }

    public async Task<GitGetInfoResult?> GetRepoInfoAsync(
        GitGetInfoConfiguration configuration,
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
            LogExtensions.GitGetInfoStarted(App.Log, configuration.Location);

            var output = await ExecuteAsync(arguments, cancellationToken);

            var lines = output.Split(Environment.NewLine);
            var hash = lines[0].Split(':', 2, StringSplitOptions.TrimEntries)[1];
            var message = lines[1].Split(':', 2, StringSplitOptions.TrimEntries)[1];
            var author = lines[2].Split(':', 2, StringSplitOptions.TrimEntries)[1];
            var email = lines[3].Split(':', 2, StringSplitOptions.TrimEntries)[1];

            LogExtensions.GitGetInfoOutput(App.Log, output);
            return new GitGetInfoResult(hash, message, author, email);
        }
        catch (Exception ex)
        {
            LogExtensions.GitGetInfoFailed(App.Log, ex);
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
            LogExtensions.GitGetBranchStarted(App.Log, configuration.Location);

            var output = await ExecuteAsync(arguments, cancellationToken);

            LogExtensions.GitGetBranchOutput(App.Log, output);

            output = Utilities.Extensions.TrimNewLine(output);

            return output;
        }
        catch (Exception ex)
        {
            LogExtensions.GitGetBranchFailed(App.Log, ex);
            return null;
        }
    }

    public async Task<IReadOnlyList<string>?> GetTagsAsync(
        GitGetTagConfiguration configuration,
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
            LogExtensions.GitGetTagStarted(App.Log, configuration.Location);

            var output = await ExecuteAsync(arguments, cancellationToken);

            LogExtensions.GitGetTagOutput(App.Log, output);

            return output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        }
        catch (Exception ex)
        {
            LogExtensions.GitGetTagFailed(App.Log, ex);
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
            LogExtensions.GitGetRootStarted(App.Log, configuration.Location);

            var output = await ExecuteAsync(arguments, cancellationToken);

            LogExtensions.GitGetRootOutput(App.Log, output);

            output = Utilities.Extensions.TrimNewLine(output);

            return output;
        }
        catch (Exception ex)
        {
            LogExtensions.GitGetRootFailed(App.Log, ex);
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
            LogExtensions.GitGetOriginUrlStarted(App.Log, configuration.Location);

            var output = await ExecuteAsync(arguments, cancellationToken);

            LogExtensions.GitGetOriginUrlOutput(App.Log, output);

            output = Utilities.Extensions.TrimNewLine(output);

            return output;
        }
        catch (Exception ex)
        {
            LogExtensions.GitGetOriginUrlFailed(App.Log, ex);
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

        LogExtensions.EnsureExitCode(process);

        return output;
    }
}
