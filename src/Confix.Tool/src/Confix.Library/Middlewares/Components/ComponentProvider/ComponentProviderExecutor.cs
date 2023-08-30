using Confix.Tool.Abstractions;
using Confix.Tool.Commands.Logging;
using Confix.Tool.Common.Pipelines;
using Confix.Tool.Entities.Components;

namespace Confix.Tool.Middlewares;

public sealed class ComponentProviderExecutor
    : IComponentProviderExecutor
{
    private readonly IReadOnlyList<IComponentProvider> _providers;

    public ComponentProviderExecutor(IReadOnlyList<IComponentProvider> providers)
    {
        _providers = providers;
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

        var providerContext =
            new ComponentProviderContext(App.Log, cancellationToken, project, solution);

        await ExecuteAsync(providerContext);

        _cache[(project, solution)] = components = providerContext.Components;

        App.Log.LogComponentsLoaded(components);

        return components;
    }

    public static IComponentProviderExecutor FromDefinitions(
        IComponentProviderFactory componentProviders,
        IEnumerable<ComponentProviderDefinition> configurations)
    {
        var providers = new List<IComponentProvider>();

        foreach (var configuration in configurations)
        {
            App.Log.LoadedComponentProvider(configuration.Type);
            providers.Add(componentProviders.CreateProvider(configuration));
        }

        if (providers.Count == 0)
        {
            App.Log.LoadedComponentProvider();
        }

        providers.Add(new MergeComponentProvider());

        return new ComponentProviderExecutor(providers);
    }
}

file static class Log
{
    public static void LoadedComponentProvider(this IConsoleLogger console, string name)
    {
        console.Debug($"Component provider '{name}' loaded");
    }

    public static void LoadedComponentProvider(this IConsoleLogger console)
    {
        console.Warning(
            "No component providers loaded because no component providers were defined. You can define component providers in the 'confix.json' or the 'confix.solution' file.");
    }

    public static void LogComponentsLoaded(
        this IConsoleLogger console,
        ICollection<Component> components)
    {
        console.Success($"Loaded {components.Count} components");
        foreach (var component in components)
        {
            console.Information($"-  @{component.Provider}/{component.ComponentName}");
        }
    }
}
