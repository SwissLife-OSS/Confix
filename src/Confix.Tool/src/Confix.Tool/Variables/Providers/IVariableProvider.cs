namespace ConfiX.Variables;

public interface IVariableProvider
{
    Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken);
    
    Task<string> ResolveAsync(string path, CancellationToken cancellationToken);
    
    Task<IReadOnlyDictionary<string, string>> ResolveManyAsync(
        IReadOnlyList<string> paths,
        CancellationToken cancellationToken);

    Task<string> SetAsync(string path, string value, CancellationToken cancellationToken);
}