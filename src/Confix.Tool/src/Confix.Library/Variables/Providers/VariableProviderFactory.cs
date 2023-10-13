using Confix.Tool;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Middlewares;

namespace Confix.Variables;

public sealed class VariableProviderFactory : IVariableProviderFactory
{
    private readonly IReadOnlyDictionary<string, Factory<IVariableProvider>> _providers;
    private readonly IServiceProvider _services;

    public VariableProviderFactory(
        IServiceProvider services,
        IReadOnlyDictionary<string, Factory<IVariableProvider>> providers)
    {
        _providers = providers;
        _services = services;
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

        return providerFactory(
            _services,
            providerConfiguration.Configuration);
    }
}
