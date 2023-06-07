using System.Text.Json.Nodes;
using Confix.Tool.Common.Pipelines;
using Confix.Utilities.Json;
using ConfiX.Variables;
using VariableProviderConfiguration = ConfiX.Variables.VariableProviderConfiguration;

namespace Confix.Tool.Middlewares;

public sealed class VariableMiddleware : IMiddleware
{
    private readonly IVariableProviderFactory _variableProviderFactory;

    public VariableMiddleware(IVariableProviderFactory variableProviderFactory)
    {
        _variableProviderFactory = variableProviderFactory;
    }

    public Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        ConfigurationFeature configurationFeature = context.Features.Get<ConfigurationFeature>();
        string environment = "local"; // TODO: read from Environment Feature

        var variableResolver = new VariableResolver(
                        _variableProviderFactory,
                        GetProviderConfiguration(configurationFeature, environment).ToArray());
        VariableResolverFeature feature = new(
            variableResolver,
            new VariableReplacerService(variableResolver));

        context.Features.Set(feature);

        return next(context);
    }

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
