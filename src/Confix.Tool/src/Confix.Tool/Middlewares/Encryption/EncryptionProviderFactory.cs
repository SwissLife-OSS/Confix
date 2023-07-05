using System.Text.Json.Nodes;
using Confix.Tool.Commands.Logging;

namespace Confix.Tool.Middlewares.Encryption;

public sealed class EncryptionProviderFactory : IEncryptionProviderFactory
{
    private readonly IReadOnlyDictionary<string, Func<JsonNode, IEncryptionProvider>> _providers;

    public EncryptionProviderFactory(IReadOnlyDictionary<string, Func<JsonNode, IEncryptionProvider>> providers)
    {
        _providers = providers;
    }

    public IEncryptionProvider CreateProvider(EncryptionProviderConfiguration providerConfiguration)
    {
        var providerFactory = _providers.GetValueOrDefault(providerConfiguration.Type);
        if (providerFactory is null)
        {
            throw new ExitException(
                $"No EncryptionProvider of type {providerConfiguration.Type.AsHighlighted()} known")
            {
                Help = "Check the documentation for a list of supported EncryptionProviders"
            };
        }

        return providerFactory(providerConfiguration.Configuration);
    }
}