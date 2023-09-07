namespace Confix.Variables;

public sealed class VariableProviderCacheException : Exception
{
    public VariableProviderCacheException(string? message) : base(message)
    {
    }

    public VariableProviderCacheException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}