using System.Text.Json.Nodes;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Json.Schema;

namespace ConfiX.Variables;

public sealed class AzureKeyVaultProvider : IVariableProvider
{
    private readonly SecretClient _client;

    public AzureKeyVaultProvider(JsonNode configuration)
        : this(AzureKeyVaultProviderConfiguration.Parse(configuration))
    { }

    public AzureKeyVaultProvider(AzureKeyVaultProviderConfiguration configuration)
        : this(new SecretClient(new Uri(configuration.Uri), new DefaultAzureCredential()))
    { }

    public AzureKeyVaultProvider(SecretClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken)
    {
        var secrets = new List<string>();
        await foreach (SecretProperties secret in _client.GetPropertiesOfSecretsAsync(cancellationToken))
        {
            secrets.Add(secret.Name.ToConfixPath());
        }
        return secrets;
    }

    public async Task<JsonNode> ResolveAsync(string path, CancellationToken cancellationToken)
    {
        KeyVaultSecret result = await _client.GetSecretAsync(path.ToKeyVaultCompatiblePath(), cancellationToken: cancellationToken);
        return JsonValue.Create(result.Value);
    }

    public Task<IReadOnlyDictionary<string, JsonNode>> ResolveManyAsync(
        IReadOnlyList<string> paths,
        CancellationToken cancellationToken)
        => paths.ResolveMany(ResolveAsync, cancellationToken);

    public async Task<string> SetAsync(string path, JsonNode value, CancellationToken cancellationToken)
    {
        if (value.GetSchemaValueType() != SchemaValueType.String)
        {
            throw new NotSupportedException("KeyVault only supports String secrets");
        }
        KeyVaultSecret result = await _client.SetSecretAsync(path.ToKeyVaultCompatiblePath(), (string)value!, cancellationToken);
        return result.Name.ToConfixPath();
    }

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
