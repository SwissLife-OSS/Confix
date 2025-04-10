using System.Text;
using Azure;
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

    public static Exception VariablesNotFound(string[] names) =>
        throw new ExitException($"Variables not found: {string.Join(", ", names)}")
        {
            Help = "Run [blue]confix variable list[/] to see all available variables."
        };

    public static Exception CouldNotParseJsonFile(FileInfo file)
        => throw new ExitException($"File {file.FullName} has invalid content.");

    public static Exception SecretNotFound(Exception innerException, string? path = null) =>
        new ExitException(
            path is null
                ? "Secret does not exist in this provider."
                : $"Secret {path.AsHighlighted()} does not exist in this provider.", innerException)
        {
            Help = $"try running {"confix variable list".AsHighlighted()} to list all available variables"
        };

    public static Exception AccessToKeyVaultFailed(RequestFailedException innerException)
    {
        var details = new StringBuilder();
        details.AppendLine($"Message: {innerException.Message}");
        details.AppendLine($"Error code: {innerException.ErrorCode}");
        details.AppendLine($"Status code: {innerException.Status}");

        return new ExitException("Access to Key Vault failed", innerException)
        {
            Help = "check if you have the required permissions to access the Key Vault",
            Details = details.ToString()
        };
    }

    public static Exception AuthenticationFailedForVault(Exception innerException) =>
        new ExitException("Authentication for Key Vault failed", innerException)
        {
            Help = $"try running {"az login".AsHighlighted()} to authenticate with Azure"
        };
}