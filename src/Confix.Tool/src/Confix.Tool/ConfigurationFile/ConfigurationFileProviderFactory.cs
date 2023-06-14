using Confix.Tool.Abstractions;
using Factory =
    System.Func<System.Text.Json.Nodes.JsonNode,
        Confix.Tool.Middlewares.IConfigurationFileProvider>;

namespace Confix.Tool.Middlewares;

public sealed class ConfigurationFileProviderFactory : IConfigurationFileProviderFactory
{
    private readonly IReadOnlyDictionary<string, Factory> _lookup;

    public ConfigurationFileProviderFactory(IReadOnlyDictionary<string, Factory> lookup)
    {
        _lookup = lookup;
    }

    public IConfigurationFileProvider Create(ConfigurationFileDefinition definition)
    {
        if (!_lookup.TryGetValue(definition.Type, out var factory))
        {
            throw new InvalidOperationException(
                $"Component file provider '{definition.Type}' is not registered.");
        }

        return factory(definition.Value);
    }
}
