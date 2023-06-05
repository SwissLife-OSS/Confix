using System.Text.Json.Nodes;

namespace ConfiX.Variables;

public sealed class VariableProviderFactory : IVariableProviderFactory
{
    private readonly IReadOnlyDictionary<string, Func<JsonNode, IVariableProvider>> _providers;

    public VariableProviderFactory(
        IReadOnlyDictionary<string, Func<JsonNode, IVariableProvider>> providers)
    {
        _providers = providers;
    }

    public IVariableProvider CreateProvider(VariableProviderConfiguration providerConfiguration) 
        => (_providers.GetValueOrDefault(providerConfiguration.Type)
                ?? throw new InvalidOperationException($"Provider {providerConfiguration.Type} not known"))
                (providerConfiguration.Configuration);

}
