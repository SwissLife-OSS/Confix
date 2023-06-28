namespace Confix.Variables;

public interface IVariableProviderFactory
{
    IVariableProvider CreateProvider(VariableProviderConfiguration providerConfiguration);
}
