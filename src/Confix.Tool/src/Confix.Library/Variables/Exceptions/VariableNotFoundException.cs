namespace Confix.Variables;

public sealed class VariableNotFoundException : Exception
{
    public VariableNotFoundException(string path) 
        : base($"Variable with path {path} could not be resolved")
    {
        Path = path;
    }
    
    public string Path { get; }
}
