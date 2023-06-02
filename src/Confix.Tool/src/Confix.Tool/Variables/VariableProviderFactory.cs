using System.Text.Json.Nodes;
namespace ConfiX.Variables;

public sealed class VariableProviderFactory : IVariableProviderFactory
{
    private readonly Dictionary<string, Func<JsonNode, IVariableProvider>> _providers;

    public VariableProviderFactory(Dictionary<string, Func<JsonNode, IVariableProvider>> providers)
    {
        _providers = providers;
    }

    public IVariableProvider CreateProvider(string providerType, JsonNode configuration)
        => (_providers.GetValueOrDefault(providerType)
            ?? throw new InvalidOperationException($"Provider {providerType} not known"))
            (configuration);
}