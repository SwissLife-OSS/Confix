namespace ConfiX.Variables;

public sealed class VariablePathParseException: Exception{
    public VariablePathParseException(string variableName)
        : base($"Variable could not be parsed. Must be of format $<provider-name>:<variable-name> but was '{variableName}'"){}
}