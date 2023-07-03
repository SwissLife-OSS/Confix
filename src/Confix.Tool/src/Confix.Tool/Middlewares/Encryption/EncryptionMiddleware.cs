using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Middlewares.Encryption;

public sealed class EncryptionMiddleware : IMiddleware
{
    private readonly IEncryptionProviderFactory _encryptionProviderFactory;

    public EncryptionMiddleware(IEncryptionProviderFactory encryptionProviderFactory)
    {
        _encryptionProviderFactory = encryptionProviderFactory;
    }

    public Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        ConfigurationFeature configurationFeature = context.Features.Get<ConfigurationFeature>();
        EnvironmentFeature environmentFeature = context.Features.Get<EnvironmentFeature>();
        string environmentName = environmentFeature.ActiveEnvironment.Name;

        var configuration = GetProviderConfiguration(configurationFeature, environmentName);
        var provider = _encryptionProviderFactory.CreateProvider(configuration);

        EncryptionFeature feature = new(provider);
        context.Features.Set(feature);

        return next(context);
    }

    private static EncryptionProviderConfiguration GetProviderConfiguration(
        ConfigurationFeature configurationFeature,
        string environmentName)
    {
        if (configurationFeature.Encryption is null)
        {
            throw new ExitException("Encryption Provider not set");
        }

        var providerDefinition = configurationFeature.Encryption.Provider;
        return new EncryptionProviderConfiguration
        {
            Type = providerDefinition.Type,
            Configuration = providerDefinition.ValueWithOverrides(environmentName)
        };
    }
}
