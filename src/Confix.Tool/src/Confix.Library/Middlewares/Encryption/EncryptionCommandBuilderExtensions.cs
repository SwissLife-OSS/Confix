using System.CommandLine.Builder;
using System.Text.Json.Nodes;
using Confix.Tool.Middlewares.Encryption.Providers.Aes;
using Confix.Tool.Middlewares.Encryption.Providers.AzureKeyvault;
using Microsoft.Extensions.DependencyInjection;

namespace Confix.Tool.Middlewares.Encryption;

public static class EncryptionCommandBuilderExtensions
{
    private static Context.Key<Dictionary<string, Func<JsonNode, IEncryptionProvider>>> _key =
        new("Confix.Tool.Entites.Encryption.EncryptionProviders");

    public static CommandLineBuilder RegisterEncryptionMiddleware(this CommandLineBuilder builder)
    {
        builder.AddDefaultVariableProviders();
        builder.AddTransient(sp
            => new OptionalEncryptionMiddleware(
                sp.GetRequiredService<IEncryptionProviderFactory>()));
        builder.AddTransient(sp
            => new EncryptionMiddleware(sp.GetRequiredService<IEncryptionProviderFactory>()));

        return builder;
    }

    private static CommandLineBuilder AddDefaultVariableProviders(this CommandLineBuilder builder)
    {
        builder.AddEncryptionProvider(
            config => new AzureKeyVaultEncryptionProvider(config));
        builder.AddEncryptionProvider(
            config => new AesEncryptionProvider(config));

        return builder;
    }

    private static CommandLineBuilder AddEncryptionProvider<T>(
        this CommandLineBuilder builder,
        Func<JsonNode, T> factory)
        where T : IEncryptionProvider
    {
        builder.GetEncryptionProviderLookup().Add(T.Type, c => factory(c));

        return builder;
    }

    private static Dictionary<string, Func<JsonNode, IEncryptionProvider>>
        GetEncryptionProviderLookup(this CommandLineBuilder builder)
    {
        var contextData = builder.GetContextData();
        if (!contextData.TryGetValue(_key, out var lookup))
        {
            lookup = new Dictionary<string, Func<JsonNode, IEncryptionProvider>>();
            contextData.Set(_key, lookup);

            builder.AddSingleton<IEncryptionProviderFactory>(
                _ => new EncryptionProviderFactory(lookup));
        }

        return lookup;
    }
}
