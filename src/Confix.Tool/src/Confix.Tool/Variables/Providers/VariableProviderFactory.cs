using System.Collections.Generic;
using System.Text.Json.Nodes;
namespace ConfiX.Variables;

public sealed class VariableProviderFactory : IVariableProviderFactory
{
    private readonly IReadOnlyDictionary<string, Func<JsonNode, IVariableProvider>> _providers;
    private readonly IReadOnlyList<VariableProviderConfiguration> _configurations;

    public VariableProviderFactory(
        IReadOnlyDictionary<string, Func<JsonNode, IVariableProvider>> providers,
        IReadOnlyList<VariableProviderConfiguration> configurations)
    {
        _providers = providers;
        _configurations = configurations;
    }

    public IVariableProvider CreateProvider(string providerName)
    {
        VariableProviderConfiguration config = GetProviderConfiguration(providerName);
        return (_providers.GetValueOrDefault(config.Type)
            ?? throw new InvalidOperationException($"Provider {config.Type} not known"))
            (config.Configuration);
    }

    private VariableProviderConfiguration GetProviderConfiguration(string providerName)
        => _configurations.FirstOrDefault(c => c.Name.Equals(providerName))
            ?? throw new InvalidOperationException($"Provider '{providerName}' not found");
}
