namespace ConfiX.Variables;

public interface IVariableResolver
{
    Task<string> ResolveVariable(VariablePath key, CancellationToken cancellationToken);
    Task<IReadOnlyDictionary<VariablePath, string>> ResolveVariables(IReadOnlyList<VariablePath> keys, CancellationToken cancellationToken);
}
