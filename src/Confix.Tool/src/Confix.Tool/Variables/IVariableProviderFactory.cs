using System.Text.Json.Nodes;
namespace ConfiX.Variables;

public interface IVariableProviderFactory
{
    IVariableProvider CreateProvider(string providerType, JsonNode configuration);
}
