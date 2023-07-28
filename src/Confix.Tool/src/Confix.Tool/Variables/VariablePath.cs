using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Confix.Tool;

namespace ConfiX.Variables;

public partial record struct VariablePath(string ProviderName, string Path)
{
    public static VariablePath Parse(string variable)
    {
        if (TryParse(variable, out var parsed))
        {
            return parsed.Value;
        }

        throw ThrowHelper.InvalidVariableName(variable);
    }

    public static bool TryParse(string variable, [NotNullWhen(true)] out VariablePath? parsed)
    {
        var x = VariableNameRegex().Match(variable);
        if (!x.Success)
        {
            parsed = null;
            return false;
        }

        parsed = new VariablePath(
            x.Groups[VariableProviderCaptureGroup].Value,
            x.Groups[VariableNameCaptureGroup].Value);
        return true;
    }

    public override string ToString() => $"${ProviderName}:{Path}";

    private const string VariableProviderCaptureGroup = "VariableProvider";
    private const string VariableNameCaptureGroup = "VariableName";

    [GeneratedRegex(
        $$"""^\$(?<{{VariableProviderCaptureGroup}}>[\w\.]+):(?<{{VariableNameCaptureGroup}}>[\w\.]+)$""")]
    private static partial Regex VariableNameRegex();
}

public static partial class VariablePathExtensions
{
    public static IEnumerable<VariablePath> GetVariables(this string value)
        => MultipleInterpolatedVariablesRegex()
            .Matches(value)
            .Select(match => VariablePath.Parse(match.Groups[VariableCaptureGroup].Value));

    public static string ReplaceVariables(this string value, Func<VariablePath, string> replacer)
        => MultipleInterpolatedVariablesRegex()
            .Replace(value,
                match =>
                {
                    var parsed = VariablePath.Parse(match.Groups[VariableCaptureGroup].Value);
                    return replacer(parsed);
                });

    private const string VariableCaptureGroup = "variable";

    [GeneratedRegex($$"""(\{\{)?(?<{{VariableCaptureGroup}}>\$(?:[\w\.]+):(?:[\w\.]+))(\}\})?""")]
    private static partial Regex MultipleInterpolatedVariablesRegex();
}
