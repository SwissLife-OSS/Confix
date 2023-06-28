using System.Diagnostics.CodeAnalysis;

namespace ConfiX.Variables;

public readonly record struct VariablePath(string ProviderName, string Path, string? Suffix = null)
{
    public static VariablePath Parse(string variable)
    {
        if (TryParse(variable, out var parsed))
        {
            return parsed.Value;
        }
        throw new VariablePathParseException(variable);
    }

    public static bool TryParse(string variable, [NotNullWhen(true)]out VariablePath? parsed)
    {
        var split = variable.Split(':') ?? Array.Empty<string>();
        if (!variable.StartsWith('$') || split.Length < 2)
        {
            parsed = null;
            return false;
        }

        parsed = new VariablePath(split[0].Remove(0, 1), split[1]);

        if(split.Length > 2)
        {
            parsed = parsed.Value with { Suffix = string.Join(':', split.Skip(2)) };
        }
        return true;
    }

    public override string ToString() => $"${ProviderName}:{Path}{(Suffix is not null ? $":{Suffix}" : "")}";
}
