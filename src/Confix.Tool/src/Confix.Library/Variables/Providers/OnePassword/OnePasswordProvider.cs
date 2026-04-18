using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Tool;
using Confix.Tool.Commands.Logging;
using Json.Schema;

namespace Confix.Variables;

public sealed class OnePasswordProvider : IVariableProvider
{
    private readonly OnePasswordProviderDefinition _definition;
    private readonly IOnePasswordCli _cli;

    public OnePasswordProvider(JsonNode configuration)
        : this(OnePasswordProviderConfiguration.Parse(configuration))
    {
    }

    public OnePasswordProvider(OnePasswordProviderConfiguration configuration)
        : this(OnePasswordProviderDefinition.From(configuration))
    {
    }

    public OnePasswordProvider(OnePasswordProviderDefinition definition)
        : this(definition, new OnePasswordCli(definition.ServiceAccountToken, definition.Account))
    {
    }

    public OnePasswordProvider(OnePasswordProviderDefinition definition, IOnePasswordCli cli)
    {
        _definition = definition;
        _cli = cli;
    }

    public static string Type => "onepassword";

    public Task<IReadOnlyList<string>> ListAsync(IVariableProviderContext context)
        => OnePasswordErrorHandler.HandleCliException<IReadOnlyList<string>>(async () =>
        {
            App.Log.Information(
                $"Listing all items from 1Password vault '{_definition.Vault}'");

            var items = await _cli.ListItemsAsync(_definition.Vault,
                context.CancellationToken);

            var paths = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item.Title))
                {
                    paths.Add(item.Title);
                }

                if (!string.IsNullOrEmpty(item.Id))
                {
                    paths.Add(item.Id);
                }
            }

            return (IReadOnlyList<string>)paths.ToList();
        });

    public Task<JsonNode> ResolveAsync(string path, IVariableProviderContext context)
        => OnePasswordErrorHandler.HandleCliException<JsonNode>(async () =>
        {
            var (item, field) = ParsePath(path);

            var value = await _cli.ReadAsync(_definition.Vault, item, field,
                context.CancellationToken);

            return JsonValue.Create(value)!;
        }, path);

    public Task<IReadOnlyDictionary<string, JsonNode>> ResolveManyAsync(
        IReadOnlyList<string> paths,
        IVariableProviderContext context)
        => paths.ResolveMany(ResolveAsync, context);

    public Task<string> SetAsync(string path, JsonNode value, IVariableProviderContext context)
        => OnePasswordErrorHandler.HandleCliException(async () =>
        {
            if (value.GetSchemaValueType() != SchemaValueType.String)
            {
                throw new NotSupportedException(
                    "1Password only supports String values");
            }

            var (item, field) = ParsePath(path);
            var stringValue = (string)value!;

            try
            {
                await _cli.EditItemFieldAsync(_definition.Vault, item, field,
                    stringValue, context.CancellationToken);
            }
            catch (OnePasswordCliException)
            {
                await _cli.CreateItemAsync(_definition.Vault, item, field,
                    stringValue, context.CancellationToken);
            }

            return path;
        }, path);

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    private const string DefaultField = "password";

    private static (string Item, string Field) ParsePath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new VariableNotFoundException(path);
        }

        var dotIndex = path.IndexOf('.');
        if (dotIndex < 0)
        {
            return (path, DefaultField);
        }

        if (dotIndex == 0 || dotIndex == path.Length - 1)
        {
            throw new VariableNotFoundException(path);
        }

        return (path[..dotIndex], path[(dotIndex + 1)..]);
    }
}

file static class OnePasswordErrorHandler
{
    public static async Task<T> HandleCliException<T>(
        Func<Task<T>> action,
        string? path = null)
    {
        try
        {
            return await action();
        }
        catch (Win32Exception)
        {
            throw new ExitException(
                "The 1Password CLI (op) could not be found.")
            {
                Help = "Install the 1Password CLI: https://developer.1password.com/docs/cli/get-started/"
            };
        }
        catch (OnePasswordCliException ex)
            when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
                  ex.Message.Contains("isn't an item", StringComparison.OrdinalIgnoreCase))
        {
            throw new VariableNotFoundException(path ?? string.Empty);
        }
        catch (OnePasswordCliException ex)
            when (ex.Message.Contains("authentication", StringComparison.OrdinalIgnoreCase) ||
                  ex.Message.Contains("unauthorized", StringComparison.OrdinalIgnoreCase))
        {
            throw new ExitException("Authentication to 1Password failed.", ex)
            {
                Help = "Check your service account token or run 'op signin' to authenticate."
            };
        }
        catch (OnePasswordCliException ex)
        {
            throw new ExitException(
                $"1Password CLI failed with exit code {ex.ExitCode}.", ex)
            {
                Details = ex.Message
            };
        }
    }

    public static async Task<string> HandleCliException(
        Func<Task<string>> action,
        string? path = null)
    {
        return await HandleCliException<string>(action, path);
    }
}
