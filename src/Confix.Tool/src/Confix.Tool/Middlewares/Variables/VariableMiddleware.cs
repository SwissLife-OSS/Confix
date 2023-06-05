using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
using Confix.Tool.Common.Pipelines;
using Confix.Utilities.Json;
using ConfiX.Variables;
using VariableProviderConfiguration = ConfiX.Variables.VariableProviderConfiguration;

namespace Confix.Tool.Middlewares;

public sealed class VariableMiddleware : IMiddleware
{
    public Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        ConfigurationFeature configurationFeature = context.Features.Get<ConfigurationFeature>();
        string environment = "local"; // TODO: read from Environment Feature

        VariableProviderFactory factory = new(
            GetVariableProviders(),
            GetProviderConfiguration(configurationFeature, environment).ToArray());

        VariableResolverFeature feature = new(new VariableResolver(factory));
        context.Features.Set(feature);

        return next(context);
    }

    private static Dictionary<string, Func<JsonNode, IVariableProvider>> GetVariableProviders()
        => new(){
            {"local", (node) => new LocalVariableProvider(node)}
        };

    private static IEnumerable<VariableProviderConfiguration> GetProviderConfiguration(
        ConfigurationFeature configurationFeature,
        string environmentName)
    {
        if (configurationFeature.Project is null) { yield break; }

        foreach (var provider in configurationFeature.Project.VariableProviders)
        {
            JsonNode config = provider.Value;

            if (provider.EnvironmentOverrides.GetValueOrDefault(environmentName) is { } envOverride)
            {
                config = config.Merge(envOverride)!;
            }

            yield return new VariableProviderConfiguration
            {
                Name = provider.Name,
                Type = provider.Type,
                Configuration = config
            };
        }
    }
}

public sealed class VariableResolverFeature
{
    public VariableResolverFeature(IVariableResolver resolver)
    {
        Resolver = resolver;
    }

    public IVariableResolver Resolver { get; }
}