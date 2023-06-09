using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;

namespace Confix.Tool.Entities.Components;

public sealed class ComponentProviderFactory
    : IComponentProviderFactory
{
    private readonly IReadOnlyDictionary<string, Func<JsonNode, IComponentProvider>> _providers;

    public ComponentProviderFactory(
        IReadOnlyDictionary<string, Func<JsonNode, IComponentProvider>> providers)
    {
        _providers = providers;
    }

    public IComponentProvider CreateProvider(ComponentProviderDefinition definition)
    {
        if (!_providers.TryGetValue(definition.Type, out var factory))
        {
            throw new InvalidOperationException(
                $"Component input type '{definition.Type}' is not registered.");
        }

        return factory(definition.Value);
    }
}
