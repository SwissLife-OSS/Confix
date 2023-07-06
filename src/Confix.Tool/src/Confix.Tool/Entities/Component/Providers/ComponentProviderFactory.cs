using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;

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
            throw new ExitException(
                $"Component input type '{definition.Type.AsHighlighted()}' is not known.")
                {Help = "Check the documentation for a list of supported component inputs."};
        }

        return factory(definition.Value);
    }
}
