namespace Confix.Variables;

public sealed class CircularVariableReferenceException : Exception
{
    public CircularVariableReferenceException(VariablePath path) 
        : base($"Variable {path} could not be resolved, due to circular references")
    {
        Path = path;
    }
    
    public VariablePath Path { get; }
}
