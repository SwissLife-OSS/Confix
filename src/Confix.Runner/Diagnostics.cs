namespace Confix.Runner;

internal static class Diagnostics
{
    /// <summary>
    /// Failures raised by application code may carry configuration values, so their text is
    /// only shown when it is explicitly asked for.
    /// </summary>
    public static string Detail(Exception failure)
    {
        return Environment.GetEnvironmentVariable("CONFIX_DIAGNOSTICS") is "1" or "true"
            ? $"\n{failure}"
            : "\nSet CONFIX_DIAGNOSTICS=1 to see the application's exception.";
    }
}
