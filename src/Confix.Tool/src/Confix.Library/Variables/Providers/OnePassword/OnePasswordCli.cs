using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Tool;

namespace Confix.Variables;

public sealed class OnePasswordCli : IOnePasswordCli
{
    private readonly string _serviceAccountToken;
    private string? _resolvedToken;

    public OnePasswordCli(string serviceAccountToken)
    {
        _serviceAccountToken = serviceAccountToken;
    }

    public async Task<string> ReadAsync(
        string vault,
        string item,
        string field,
        CancellationToken cancellationToken)
    {
        var result = await ExecuteAsync(
            ["read", $"op://{vault}/{item}/{field}"],
            cancellationToken);

        return result.Trim();
    }

    public async Task<IReadOnlyList<OnePasswordItemSummary>> ListItemsAsync(
        string vault,
        CancellationToken cancellationToken)
    {
        var json = await ExecuteAsync(
            ["item", "list", "--vault", vault, "--format", "json", "--no-color"],
            cancellationToken);

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

    public async Task<OnePasswordItemDetail> GetItemAsync(
        string vault,
        string item,
        CancellationToken cancellationToken)
    {
        var json = await ExecuteAsync(
            ["item", "get", item, "--vault", vault, "--format", "json", "--no-color"],
            cancellationToken);

        var node = JsonNode.Parse(json)!;
        var id = node["id"]?.GetValue<string>() ?? string.Empty;
        var title = node["title"]?.GetValue<string>() ?? string.Empty;

        var fields = new List<OnePasswordFieldInfo>();
        if (node["fields"] is JsonArray fieldsArray)
        {
            foreach (var field in fieldsArray)
            {
                if (field is null)
                {
                    continue;
                }

                var fieldId = field["id"]?.GetValue<string>() ?? string.Empty;
                var label = field["label"]?.GetValue<string>() ?? string.Empty;
                var value = field["value"]?.GetValue<string>();
                fields.Add(new OnePasswordFieldInfo(fieldId, label, value));
            }
        }

        return new OnePasswordItemDetail(id, title, fields);
    }

    public async Task EditItemFieldAsync(
        string vault,
        string item,
        string field,
        string value,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(
            ["item", "edit", item, "--vault", vault, $"{field}={value}"],
            cancellationToken);
    }

    public async Task CreateItemAsync(
        string vault,
        string item,
        string field,
        string value,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(
            [
                "item", 
                "create", 
                "--vault", 
                vault, 
                "--title", 
                item,
                "--category", 
                "login", 
                $"{field}={value}"
            ],
            cancellationToken);
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
            CreateNoWindow = true
        };

        foreach (var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        startInfo.Environment["OP_SERVICE_ACCOUNT_TOKEN"] = token;

        var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start op process.");

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

    private string ResolveToken()
    {
        if (_resolvedToken is not null)
        {
            return _resolvedToken;
        }

        if (_serviceAccountToken.StartsWith('$'))
        {
            var envVarName = _serviceAccountToken[1..];
            _resolvedToken = Environment.GetEnvironmentVariable(envVarName)
                ?? throw new ExitException(
                    $"Environment variable '{envVarName}' is not set.")
                {
                    Help = $"Set the environment variable or provide the token directly in the provider configuration."
                };
        }
        else
        {
            _resolvedToken = _serviceAccountToken;
        }

        return _resolvedToken;
    }
}
