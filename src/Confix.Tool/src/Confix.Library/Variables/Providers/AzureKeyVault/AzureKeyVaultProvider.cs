using System.Text.Json.Nodes;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Confix.Tool.Commands.Logging;
using Confix.Utilities.Azure;
using Json.Schema;

namespace Confix.Variables;

public sealed class AzureKeyVaultProvider : IVariableProvider
{
    private readonly SecretClient _client;

    public AzureKeyVaultProvider(JsonNode configuration)
        : this(AzureKeyVaultProviderConfiguration.Parse(configuration))
    {
    }

    public AzureKeyVaultProvider(AzureKeyVaultProviderConfiguration configuration)
        : this(AzureKeyVaultProviderDefinition.From(configuration))
    {
    }

    public AzureKeyVaultProvider(AzureKeyVaultProviderDefinition definition)
        : this(new SecretClient(new Uri(definition.Uri), new AzureCliCredential()))
    {
    }

    public AzureKeyVaultProvider(SecretClient client)
    {
        _client = client;
    }

    /// <inheritdoc />
    public static string Type => "azure-keyvault";

    public Task<IReadOnlyList<string>> ListAsync(IVariableProviderContext context)
        => KeyVaultExtension.HandleKeyVaultException<IReadOnlyList<string>>(async () =>
        {
            App.Log.ListSecrets(_client.VaultUri);

            var secrets = new List<string>();
            await foreach (var secret in _client.GetPropertiesOfSecretsAsync(context.CancellationToken))
            {
                secrets.Add(secret.Name.ToConfixPath());
            }

            return secrets;
        });

    public Task<JsonNode> ResolveAsync(string path, IVariableProviderContext context)
        => KeyVaultExtension.HandleKeyVaultException<JsonNode>(async () =>
        {
            KeyVaultSecret result = await _client.GetSecretAsync(path.ToKeyVaultCompatiblePath(),
                cancellationToken: context.CancellationToken);
            return JsonValue.Create(result.Value)!;
        }, path);

    public Task<IReadOnlyDictionary<string, JsonNode>> ResolveManyAsync(
        IReadOnlyList<string> paths,
        IVariableProviderContext context)
        => paths.ResolveMany(ResolveAsync, context);

    public Task<string> SetAsync(string path, JsonNode value, IVariableProviderContext context)
        => KeyVaultExtension.HandleKeyVaultException(async () =>
        {
            if (value.GetSchemaValueType() != SchemaValueType.String)
            {
                throw new NotSupportedException("KeyVault only supports String secrets");
            }

            KeyVaultSecret result = await _client
                .SetSecretAsync(path.ToKeyVaultCompatiblePath(), (string)value!, context.CancellationToken);
            return result.Name.ToConfixPath();
        }, path);

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

file static class Extensions
{
    public static string ToConfixPath(this string path) => path.Replace('-', '.');

    public static string ToKeyVaultCompatiblePath(this string path) => path.Replace('.', '-');
}

file static class LogExtensions
{
    public static void ListSecrets(this IConsoleLogger log, Uri vaultUri)
    {
        log.Information($"List all secrets from Azure Kev Vault '{vaultUri}'");
    }
}