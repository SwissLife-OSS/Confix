using Azure;
using Azure.Identity;
using Confix.Tool;
using Confix.Tool.Commands.Logging;

namespace Confix.Utilities.Azure
{
    public static class KeyVaultExtension
    {
        public static async Task<T> HandleKeyVaultException<T>(Func<Task<T>> action)
        {
            try
            {
                return await action();
            }
            catch (RequestFailedException ex)
            {
                throw new ExitException("Access to Key Vault failed", ex)
                {
                    Help = "check if you have the required permissions to access the Key Vault"
                };
            }
            catch (AuthenticationFailedException ex)
            {
                throw new ExitException("Authentication for Key Vault failed", ex)
                {
                    Help = $"try running {"az login".AsHighlighted()} to authenticate with Azure"
                };
            }
        }
    }
}