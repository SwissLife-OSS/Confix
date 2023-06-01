namespace ConfiX.Variables;

public class VariableNotFoundException : Exception
{
    public VariableNotFoundException(string path) : base($"Variable with path {path} could not be resolved")
    {
    }
}
