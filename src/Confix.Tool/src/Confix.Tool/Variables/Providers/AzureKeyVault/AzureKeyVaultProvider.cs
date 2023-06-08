using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace ConfiX.Variables;

public sealed class AzureKeyVaultProvider : IVariableProvider
{
    private readonly AzureKeyVaultProviderConfiguration _configuration;
    private readonly SecretClient _client;

    public AzureKeyVaultProvider(JsonNode configuration)
        : this(AzureKeyVaultProviderConfiguration.Parse(configuration))
    { }

    public AzureKeyVaultProvider(AzureKeyVaultProviderConfiguration configuration)
    {
        _configuration = configuration;
        _client = new SecretClient(new Uri(_configuration.Uri), new DefaultAzureCredential());
    }

    public async Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken)
    {
        var secrets = new List<string>();
        await foreach (SecretProperties secret in _client.GetPropertiesOfSecretsAsync(cancellationToken))
        {
            secrets.Add(secret.Name);
        }
        return secrets;
    }

    public async Task<JsonValue> ResolveAsync(string path, CancellationToken cancellationToken)
    {
        KeyVaultSecret result = await _client.GetSecretAsync(path, cancellationToken: cancellationToken);
        return JsonValue.Create(result.Value);
    }

    public async Task<IReadOnlyDictionary<string, JsonValue>> ResolveManyAsync(
        IReadOnlyList<string> paths,
        CancellationToken cancellationToken)
        => new Dictionary<string, JsonValue>(await Task.WhenAll(
            paths.Select(async path => new KeyValuePair<string, JsonValue>(
                path,
                await ResolveAsync(path, cancellationToken)))));

    public async Task<string> SetAsync(string path, JsonValue value, CancellationToken cancellationToken)
    {
        if (value.GetValue<JsonElement>().ValueKind != JsonValueKind.String)
        {
            throw new NotSupportedException("KeyVault only supports String secrets");
        }
        await _client.SetSecretAsync(path, (string)value!, cancellationToken);
        return path;
    }
}
