using System.Text.Json.Nodes;
using Confix.Tool;
using Confix.Tool.Commands.Logging;

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
    {
        var providerFactory = _providers.GetValueOrDefault(providerConfiguration.Type);
        if (providerFactory is null)
        {
            throw new ExitException(
                $"No VariableProvider of type {providerConfiguration.Type.AsHighlighted()} known")
            {
                Help = "Check the documentation for a list of supported VariableProviders"
            };
        }
        return providerFactory(providerConfiguration.Configuration);
    }

}
