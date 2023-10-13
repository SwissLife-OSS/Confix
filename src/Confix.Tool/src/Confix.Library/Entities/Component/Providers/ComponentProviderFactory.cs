using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Middlewares;

namespace Confix.Tool.Entities.Components;

public sealed class ComponentProviderFactory
    : IComponentProviderFactory
{
    private readonly IServiceProvider _services;
    private readonly IReadOnlyDictionary<string, Factory<IComponentProvider>> _providers;

    public ComponentProviderFactory(
        IServiceProvider services,
        IReadOnlyDictionary<string, Factory<IComponentProvider>> providers)
    {
        _providers = providers;
        _services = services;
    }

    public IComponentProvider CreateProvider(ComponentProviderDefinition definition)
    {
        if (!_providers.TryGetValue(definition.Type, out var factory))
        {
            throw new ExitException(
                    $"Component input type '{definition.Type.AsHighlighted()}' is not known.")
                { Help = "Check the documentation for a list of supported component inputs." };
        }

        return factory(_services, definition.Value);
    }
}
