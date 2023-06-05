namespace ConfiX.Variables;

public interface IVariableProviderFactory
{
    IVariableProvider CreateProvider(string providerName);
}
