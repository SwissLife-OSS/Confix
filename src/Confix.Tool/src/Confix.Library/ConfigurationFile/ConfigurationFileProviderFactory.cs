using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
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
            throw new ExitException(
                $"No Component file provider of type {definition.Type.AsHighlighted()} known")
            {
                Help = "Check the documentation for a list of supported Component file providers"
            };
        }

        return factory(definition.Value);
    }
}
