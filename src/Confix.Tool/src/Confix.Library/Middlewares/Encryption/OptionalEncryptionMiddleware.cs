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
        var configurationFeature = context.Features.Get<ConfigurationFeature>();
        if (configurationFeature.Encryption is { Provider: var providerDefinition })
        {
            context.Features.TryGet<EnvironmentFeature>(out var environmentFeature);
            var environmentName = environmentFeature?.ActiveEnvironment.Name;

            var configuration = new EncryptionProviderConfiguration
            {
                Type = providerDefinition.Type,
                Configuration = environmentName != null
                    ? providerDefinition.ValueWithOverrides(environmentName)
                    : providerDefinition.Value
            };
            var provider = _encryptionProviderFactory.CreateProvider(configuration);

            EncryptionFeature feature = new(provider);
            context.Features.Set(feature);
        }

        return next(context);
    }
}
