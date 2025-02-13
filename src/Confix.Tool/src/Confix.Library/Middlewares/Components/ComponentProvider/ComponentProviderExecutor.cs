using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Entities.Components;
using Confix.Tool.Entities.Components.Local;

namespace Confix.Tool.Middlewares;

public sealed class ComponentProviderExecutor
    : IComponentProviderExecutor, IAsyncDisposable
{
    private readonly IReadOnlyList<IComponentProvider> _providers;
    private readonly IParameterCollection _parameters;

    public ComponentProviderExecutor(
        IReadOnlyList<IComponentProvider> providers,
        IParameterCollection parameters)
    {
        _providers = providers;
        _parameters = parameters;
    }

    public async Task ExecuteAsync(IComponentProviderContext context)
    {
        foreach (var provider in _providers)
        {
            await provider.ExecuteAsync(context);
        }
    }

    private readonly Dictionary<(ProjectDefinition, SolutionDefinition), IList<Component>> _cache =
        new();

    public async Task<IList<Component>> LoadComponents(
        SolutionDefinition solution,
        ProjectDefinition project,
        CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue((project, solution), out var components))
        {
            return components;
        }

        var providerContext = new ComponentProviderContext(
            App.Log,
            cancellationToken,
            project,
            solution,
            _parameters,
            project.Components);

        await ExecuteAsync(providerContext);

        _cache[(project, solution)] = components = providerContext.Components;

        App.Log.LogComponentsLoaded(components);

        return components;
    }

    public static IComponentProviderExecutor FromDefinitions(
        IComponentProviderFactory componentProviders,
        IEnumerable<ComponentProviderDefinition> configurations,
        IParameterCollection parameters)
    {
        var providers = new List<IComponentProvider>();

        foreach (var configuration in configurations)
        {
            App.Log.LoadedComponentProvider(configuration.Type);
            providers.Add(componentProviders.CreateProvider(configuration));
        }

        providers.Add(new LocalComponentProvider());
        providers.Add(new MergeComponentProvider());


        return new ComponentProviderExecutor(providers, parameters);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        foreach (var provider in _providers)
        {
            if (provider is IAsyncDisposable disposable)
            {
                await disposable.DisposeAsync();
            }
        }
    }
}

file static class Log
{
    public static void LoadedComponentProvider(this IConsoleLogger console, string name)
    {
        console.Debug($"Component provider '{name}' loaded");
    }

    public static void LogComponentsLoaded(
        this IConsoleLogger console,
        ICollection<Component> components)
    {
        console.Success($"Loaded {components.Count} components");
        foreach (var component in components)
        {
            console.Information($"-  {component.GetKey()}");
        }
    }
}
