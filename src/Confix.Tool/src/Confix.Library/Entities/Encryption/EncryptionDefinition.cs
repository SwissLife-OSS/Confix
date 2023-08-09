using System.Text.Json;

namespace Confix.Tool.Abstractions;

public record EncryptionDefinition(EncryptionProviderDefinition Provider)
{
    public void WriteTo(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName(EncryptionConfiguration.FieldNames.Provider);
        Provider.WriteTo(writer);
        writer.WriteEndObject();
    }

    public static EncryptionDefinition From(EncryptionConfiguration configuration)
    {
        if (configuration.Provider is null)
        {
            throw new ValidationException("Encryption configuration is invalid.")
            {
                Errors = new[] { "Provider is required." }
            };
        }

        var provider = EncryptionProviderDefinition.From(configuration.Provider!);

        return new EncryptionDefinition(provider);
    }
}
