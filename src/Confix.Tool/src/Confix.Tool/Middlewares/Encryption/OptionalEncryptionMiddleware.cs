using Confix.Tool.Common.Pipelines;

namespace Confix.Tool.Middlewares.Encryption;

public class OptionalEncryptionMiddleware : IMiddleware
{
    private readonly IEncryptionProviderFactory _encryptionProviderFactory;

    public OptionalEncryptionMiddleware(IEncryptionProviderFactory encryptionProviderFactory)
    {
        _encryptionProviderFactory = encryptionProviderFactory;
    }

    public virtual Task InvokeAsync(IMiddlewareContext context, MiddlewareDelegate next)
    {
        ConfigurationFeature configurationFeature = context.Features.Get<ConfigurationFeature>();
        if (configurationFeature.Encryption is not null)
        {
            context.Features.TryGet<EnvironmentFeature>(out EnvironmentFeature? environmentFeature);
            string? environmentName = environmentFeature?.ActiveEnvironment.Name;

            var configuration = GetProviderConfiguration(configurationFeature, environmentName);
            var provider = _encryptionProviderFactory.CreateProvider(configuration);

            EncryptionFeature feature = new(provider);
            context.Features.Set(feature);
        }
        return next(context);
    }

    private static EncryptionProviderConfiguration GetProviderConfiguration(
        ConfigurationFeature configurationFeature,
        string? environmentName)
    {
        if (configurationFeature.Encryption is null)
        {
            throw new ExitException("Encryption Provider not set");
        }

        var providerDefinition = configurationFeature.Encryption.Provider;
        return new EncryptionProviderConfiguration
        {
            Type = providerDefinition.Type,
            Configuration = environmentName != null
                ? providerDefinition.ValueWithOverrides(environmentName)
                : providerDefinition.Value
        };
    }
}