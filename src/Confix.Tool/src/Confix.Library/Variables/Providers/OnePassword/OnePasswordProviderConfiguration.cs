using System.Text.Json;
using System.Text.Json.Nodes;
using Confix.Extensions;

namespace Confix.Variables;

public sealed record OnePasswordProviderConfiguration(
    string? Vault,
    string? ServiceAccountToken
)
{
    public static OnePasswordProviderConfiguration Parse(JsonNode node)
        => node.Deserialize(JsonSerialization.Instance.OnePasswordProviderConfiguration)!;
}
