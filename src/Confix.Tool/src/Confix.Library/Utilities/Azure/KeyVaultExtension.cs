using Azure;
using Azure.Identity;
using Confix.Tool;

namespace Confix.Utilities.Azure;

public static class KeyVaultExtension
{
    public static async Task<T> HandleKeyVaultException<T>(Func<Task<T>> action, string? path = null)
    {
        try
        {
            return await action();
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "SecretNotFound")
        {
            throw ThrowHelper.SecretNotFound(ex, path);
        }
        catch (RequestFailedException ex)
        {
            throw ThrowHelper.AccessToKeyVaultFailed(ex);
        }
        catch (AuthenticationFailedException ex)
        {
            throw ThrowHelper.AuthenticationFailedForVault(ex);
        }
    }
}