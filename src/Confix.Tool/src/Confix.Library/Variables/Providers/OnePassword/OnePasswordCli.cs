using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Tool;

namespace Confix.Variables;

public sealed class OnePasswordCli : IOnePasswordCli
{
    private const string DefaultServiceAccountTokenEnvVar = "OP_SERVICE_ACCOUNT_TOKEN";
    private const string AccountEnvVar = "OP_ACCOUNT";

    private readonly string? _serviceAccountToken;
    private readonly string? _account;
    private string? _resolvedToken;
    private bool _tokenResolved;

    public OnePasswordCli(string? serviceAccountToken, string? account = null)
    {
        _serviceAccountToken = serviceAccountToken;
        _account = account;
    }

    public async Task<string> ReadAsync(
        string vault,
        string item,
        string field,
        CancellationToken cancellationToken
    )
    {
        var result = await ExecuteAsync(
            new[] { "read", $"op://{vault}/{item}/{field}" },
            cancellationToken
        );

        return result.Trim();
    }

    public async Task<IReadOnlyList<OnePasswordItemSummary>> ListItemsAsync(
        string vault,
        CancellationToken cancellationToken
    )
    {
        var json = await ExecuteAsync(
            new[] { "item", "list", "--vault", vault, "--format", "json", "--no-color" },
            cancellationToken
        );

        var items = JsonSerializer.Deserialize<JsonArray>(json) ?? new JsonArray();
        var result = new List<OnePasswordItemSummary>();
        foreach (var item in items)
        {
            if (item is null)
            {
                continue;
            }

            var id = item["id"]?.GetValue<string>() ?? string.Empty;
            var title = item["title"]?.GetValue<string>() ?? string.Empty;
            result.Add(new OnePasswordItemSummary(id, title));
        }

        return result;
    }

    public async Task EditItemFieldAsync(
        string vault,
        string item,
        string field,
        string value,
        CancellationToken cancellationToken
    )
    {
        await ExecuteAsync(
            new[] { "item", "edit", item, "--vault", vault, $"{field}={value}" },
            cancellationToken
        );
    }

    public async Task CreateItemAsync(
        string vault,
        string item,
        string field,
        string value,
        CancellationToken cancellationToken
    )
    {
        await ExecuteAsync(
            new[]
            {
                "item",
                "create",
                "--vault",
                vault,
                "--title",
                item,
                "--category",
                "login",
                $"{field}={value}",
            },
            cancellationToken
        );
    }

    private async Task<string> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        var token = ResolveToken();

        var startInfo = new ProcessStartInfo
        {
            FileName = "op",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        foreach (var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        if (token is not null)
        {
            startInfo.Environment[DefaultServiceAccountTokenEnvVar] = token;
        }

        if (!string.IsNullOrWhiteSpace(_account))
        {
            startInfo.Environment[AccountEnvVar] = _account;
        }

        using var process =
            Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start op process.");

        try
        {
            var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);

            var stdout = await stdoutTask;
            var stderr = await stderrTask;

            if (process.ExitCode != 0)
            {
                throw new OnePasswordCliException(process.ExitCode, stderr.Trim());
            }

            return stdout;
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
            throw;
        }
    }

    private string? ResolveToken()
    {
        if (_tokenResolved)
        {
            return _resolvedToken;
        }

        _resolvedToken = ResolveTokenCore();
        _tokenResolved = true;
        return _resolvedToken;
    }

    private string? ResolveTokenCore()
    {
        if (_serviceAccountToken is null)
        {
            return Environment.GetEnvironmentVariable(DefaultServiceAccountTokenEnvVar);
        }

        if (_serviceAccountToken.StartsWith('$'))
        {
            var envVarName = _serviceAccountToken[1..];
            return Environment.GetEnvironmentVariable(envVarName)
                ?? throw new ExitException($"Environment variable '{envVarName}' is not set.")
                {
                    Help =
                        "Set the environment variable, provide the token directly in the provider configuration, or remove the serviceAccountToken setting to use your existing 'op' CLI session.",
                };
        }

        return _serviceAccountToken;
    }
}
