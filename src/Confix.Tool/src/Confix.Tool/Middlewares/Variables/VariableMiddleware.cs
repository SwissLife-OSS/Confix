using Confix.Tool.Abstractions;
using Confix.Tool.Common.Pipelines;
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
        var configurationFeature = context.Features.Get<ConfigurationFeature>();
        var environmentFeature = context.Features.Get<EnvironmentFeature>();

        var environmentName = environmentFeature.ActiveEnvironment.Name;
        var variableResolver = CreateResolver(environmentName);

        var replacerService = new VariableReplacerService(variableResolver);

        var feature =
            new VariableResolverFeature(CreateResolver, variableResolver, replacerService);

        context.Features.Set(feature);

        return next(context);

        IVariableResolver CreateResolver(string environment)
        {
            var configurations =
                GetProviderConfiguration(configurationFeature, environment).ToArray();

            return new VariableResolver(_variableProviderFactory, configurations);
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
