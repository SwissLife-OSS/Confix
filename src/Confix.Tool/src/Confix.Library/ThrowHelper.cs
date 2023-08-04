using Confix.Tool.Commands.Logging;

namespace Confix.Tool;

public static class ThrowHelper
{
    public static Exception InvalidVariableName(string name)
        => new ExitException($"Invalid variable name: {name}")
        {
            Help = "Variable name must be like: [blue]$provider:some.path[/]"
        };

    public static Exception InvalidProviderName(
        IEnumerable<string> providers,
        string name)
        => throw new ExitException($"Invalid provider name: {name}")
        {
            Help = $"Available providers: {string.Join(", ", providers)}"
        };

    public static Exception EnvironmentDoesNotExist(string name)
        => new ExitException($"Environment '{name}' does not exists.")
        {
            Help = $"Use [blue]confix environment set {name}[/] to change it."
        };

    public static Exception VariableNotFound(string name) =>
        throw new ExitException($"Variable not found: {name}")
        {
            Help = "Run [blue]confix variable list[/] to see all available variables."
        };

    public static Exception CouldNotParseJsonFile(FileInfo file)
        => throw new ExitException($"File {file.FullName} has invalid content.");

    public static Exception SecretNotFound(Exception innerException) =>
        new ExitException("Secret does not exist in this provider.", innerException)
        {
            Help =
                $"try running {"confix variables list".AsHighlighted()} to list all available variables"
        };

    public static Exception AccessToKeyVaultFailed(Exception innerException) =>
        new ExitException("Access to Key Vault failed", innerException)
        {
            Help = "check if you have the required permissions to access the Key Vault"
        };

    public static Exception AuthenticationFailedForVault(Exception innerException) =>
        new ExitException("Authentication for Key Vault failed", innerException)
        {
            Help = $"try running {"az login".AsHighlighted()} to authenticate with Azure"
        };
}
