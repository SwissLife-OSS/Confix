using System.Text.Json.Nodes;

namespace Confix.Tool.Middlewares.Encryption;

public sealed record EncryptionProviderConfiguration
{
    public required string Type { get; init; }
    public required JsonNode Configuration { get; init; }
}
