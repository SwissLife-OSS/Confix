using System.Text.Json.Nodes;
namespace ConfiX.Variables;

public sealed class VariableProviderFactory : IVariableProviderFactory
{
    private readonly Dictionary<string, Func<JsonNode, IVariableProvider>> providers = new()
        {
            { LocalVariableProvider.PropertyType,(c) => new LocalVariableProvider(c) },
        };
    public IVariableProvider CreateProvider(string providerType, JsonNode configuration)
        => (providers.GetValueOrDefault(providerType) 
            ?? throw new InvalidOperationException("Provider {providerType} not known"))
            (configuration);
}