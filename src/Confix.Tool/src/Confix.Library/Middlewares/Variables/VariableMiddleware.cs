using Confix.Tool.Common.Pipelines;
using Confix.Variables;
using Microsoft.Extensions.DependencyInjection;
using VariableProviderConfiguration = Confix.Variables.VariableProviderConfiguration;

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
        var configurationFeature = context.Features.Get<ConfigurationFeature>();
        var environmentFeature = context.Features.Get<EnvironmentFeature>();

        var environmentName = environmentFeature.ActiveEnvironment.Name;
        var variableListCache = context.Services.GetRequiredService<VariableListCache>();

        var resolver = CreateResolver(environmentName, variableListCache);
        var replacer = new VariableReplacerService(resolver);
        var extractor = new VariableExtractorService(resolver);

        var feature = new VariablesFeature(CreateResolver, resolver, replacer, extractor);

        context.Features.Set(feature);

        return next(context);

        IVariableResolver CreateResolver(string environment, VariableListCache cache)
        {
            var configurations =
                GetProviderConfiguration(configurationFeature, environment).ToArray();

            return new VariableResolver(_variableProviderFactory, cache, configurations);
        }
    }

    private static IEnumerable<VariableProviderConfiguration> GetProviderConfiguration(
        ConfigurationFeature configurationFeature,
        string environmentName)
    {
        if (configurationFeature.Project is null)
        {
            yield break;
        }

        foreach (var provider in configurationFeature.Project.VariableProviders)
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
