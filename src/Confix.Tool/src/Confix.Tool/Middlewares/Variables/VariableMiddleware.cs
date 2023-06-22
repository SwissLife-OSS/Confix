using System.Text.Json.Nodes;
using Confix.Tool.Abstractions;
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
        EnvironmentFeature environmentFeature = context.Features.Get<EnvironmentFeature>();
        string environmentName = environmentFeature.ActiveEnvironment.Name;
        var providers = GetProviderConfiguration(configurationFeature, environmentName).ToArray();
        var variableResolver = new VariableResolver(_variableProviderFactory, providers);

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

        foreach (VariableProviderDefinition provider in configurationFeature.Project.VariableProviders)
        {
            yield return new VariableProviderConfiguration
            {
                Name = provider.Name,
                Type = provider.Type,
                Configuration = provider.ValueWithOverrides(environmentName)
            };
        }
    }
}