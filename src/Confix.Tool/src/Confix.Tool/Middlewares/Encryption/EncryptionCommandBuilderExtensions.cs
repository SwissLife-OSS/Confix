using System.CommandLine.Builder;
using System.Text.Json.Nodes;
using Confix.Tool.Middlewares.Encryption.Providers.AzureKeyvault;
using Confix.Tool.Middlewares.Encryption.Providers.Test;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool.Middlewares.Encryption;

public static class EncryptionCommandBuilderExtensions
{
    private const string _encryptionProviders = "Confix.Tool.Entites.Encryption.EncryptionProviders";

    public static CommandLineBuilder RegisterEncryptionMiddleware(this CommandLineBuilder builder)
    {
        builder.AddDefaultVariableProviders();
        builder.AddTransient(sp
            => new EncryptionMiddleware(sp.GetRequiredService<IEncryptionProviderFactory>()));

        return builder;
    }

    private static CommandLineBuilder AddDefaultVariableProviders(this CommandLineBuilder builder)
    {
        builder.AddEncryptionProvider(
            AzureKeyVaultEncryptionProvider.Type,
            (config) => new AzureKeyVaultEncryptionProvider(config));

        return builder;
    }

    private static CommandLineBuilder AddEncryptionProvider(
        this CommandLineBuilder builder,
        string name,
        Func<JsonNode, IEncryptionProvider> factory)
    {
        builder.GetEncryptionProviderLookup().Add(name, factory);

        return builder;
    }

    private static Dictionary<string, Func<JsonNode, IEncryptionProvider>> GetEncryptionProviderLookup(
        this CommandLineBuilder builder)
    {
        var contextData = builder.GetContextData();

        if (!contextData.TryGetValue(_encryptionProviders,
                out Dictionary<string, Func<JsonNode, IEncryptionProvider>>? lookup))
        {
            lookup = new Dictionary<string, Func<JsonNode, IEncryptionProvider>>();
            contextData.Add(_encryptionProviders, lookup);

            builder.AddSingleton<IEncryptionProviderFactory>(_
                => new EncryptionProviderFactory(lookup));
        }

        return lookup;
    }
}