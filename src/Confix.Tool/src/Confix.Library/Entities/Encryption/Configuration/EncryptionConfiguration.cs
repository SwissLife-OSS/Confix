using System.Text.Json.Nodes;
using Confix.Utilities.Json;

namespace Confix.Tool.Abstractions;

public sealed record EncryptionConfiguration(EncryptionProviderConfiguration? Provider)
{
    public static class FieldNames
    {
        public const string Provider = "provider";
    }

    public static EncryptionConfiguration Parse(JsonNode node)
    {
        var obj = node.ExpectObject();
        var provider = obj.TryGetNonNullPropertyValue(FieldNames.Provider, out var providerNode)
            ? EncryptionProviderConfiguration.Parse(providerNode.ExpectObject())
            : null;

        return new(provider);
    }

    public EncryptionConfiguration Merge(EncryptionConfiguration? other)
    {
        if (other is null)
        {
            return this;
        }

        var provider = Provider?.Merge(other.Provider) ?? other.Provider;

        return new EncryptionConfiguration(provider);
    }
}
