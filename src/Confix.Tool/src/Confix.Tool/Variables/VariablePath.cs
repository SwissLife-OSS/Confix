namespace ConfiX.Variables;

public record struct VariablePath(string ProviderName, string Path)
{
    public static VariablePath Parse(string variable)
    {
        var split = variable.Split(':') ?? Array.Empty<string>();
        if (!variable.StartsWith('$') || split.Length != 2)
        {
            throw new VariablePathParseException(variable);
        }

        return new VariablePath(split[0].Remove(0,1), split[1]);
    }
}

public sealed class VariablePathParseException: Exception{
    public VariablePathParseException(string variableName)
        : base($"Variable could not be parsed. Must be of format $<provider-name>:<variable-name> but was '{variableName}'"){}
}