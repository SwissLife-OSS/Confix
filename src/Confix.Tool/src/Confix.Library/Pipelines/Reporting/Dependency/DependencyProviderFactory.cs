using Confix.Tool.Commands.Logging;
using Confix.Tool.Middlewares;

namespace Confix.Tool.Reporting;

public sealed class DependencyProviderFactory
    : IDependencyProviderFactory
{
    private readonly IServiceProvider _services;
    private readonly IReadOnlyDictionary<string, Factory<IDependencyProvider>> _lookup;

    public DependencyProviderFactory(
        IServiceProvider services,
        IReadOnlyDictionary<string, Factory<IDependencyProvider>> lookup)
    {
        _services = services;
        _lookup = lookup;
    }

    public IDependencyProvider Create(DependencyProviderDefinition definition)
    {
        if (!_lookup.TryGetValue(definition.Type, out var factory))
        {
            throw new ExitException(
                    $"Component input type '{definition.Type.AsHighlighted()}' is not registered.")
                { Help = "Check the documentation for a list of supported component inputs." };
        }

        return factory(_services, definition.Value);
    }
}
