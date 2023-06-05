namespace ConfiX.Variables;

public interface IVariableProviderFactory
{
    IVariableProvider CreateProvider(VariableProviderConfiguration providerConfiguration);
}
