using System.Text.Json.Nodes;
using Confix.Tool.Commands.Logging;

namespace Confix.Tool.Reporting;

public sealed class DependencyAnalyzer : IDependencyAnalyzer
{
    private readonly IDependencyProvider[] _providers;

    public DependencyAnalyzer(IEnumerable<IDependencyProvider> providers)
    {
        _providers = providers.ToArray();
    }

    public void Analyze(DependencyAnalyzerContext context, JsonNode node)
    {
        for (var i = 0; i < _providers.Length; i++)
        {
            _providers[i].Analyze(context, node);
        }
    }

    public bool HasProviders() => _providers.Length > 0;

    public static IDependencyAnalyzer FromDefinitions(
        IDependencyProviderFactory factory,
        IEnumerable<DependencyProviderDefinition> configurations)
    {
        var providers = new List<IDependencyProvider>();

        foreach (var configuration in configurations)
        {
            App.Log.LoadedDependencyProvider(configuration.Type);
            providers.Add(factory.Create(configuration));
        }

        return new DependencyAnalyzer(providers);
    }
}

file static class Log
{
    public static void LoadedDependencyProvider(this IConsoleLogger console, string name)
    {
        console.Debug($"Dependency provider '{name}' loaded");
    }
}
